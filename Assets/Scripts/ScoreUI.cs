using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SliceShoot.Core;

namespace SliceShoot.UI
{
    public class ScoreUI : MonoBehaviour
    {
        private TextMeshProUGUI _scoreText;
        private float _animTimer = -1f;

        private void Start()
        {
            CreateUI();

            ScoreManager.Instance.OnScoreChanged += OnScoreChanged;
            ScoreManager.Instance.OnComboHit += OnComboHit;
            ScoreManager.Instance.OnComboBreak += OnComboBreak;
        }

        private void OnDestroy()
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged -= OnScoreChanged;
                ScoreManager.Instance.OnComboHit -= OnComboHit;
                ScoreManager.Instance.OnComboBreak -= OnComboBreak;
            }
        }

        private void CreateUI()
        {
            // Canvas
            var canvasGO = new GameObject("ScoreCanvas");
            canvasGO.transform.SetParent(transform);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // Score text
            var textGO = new GameObject("ScoreText");
            textGO.transform.SetParent(canvasGO.transform, false);

            _scoreText = textGO.AddComponent<TextMeshProUGUI>();
            _scoreText.text = "0";
            _scoreText.fontSize = 48;
            _scoreText.color = Color.white;
            _scoreText.alignment = TextAlignmentOptions.Center;
            _scoreText.fontStyle = FontStyles.Bold;
            _scoreText.outlineWidth = 0.2f;
            _scoreText.outlineColor = Color.black;

            var rect = _scoreText.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -30f);
            rect.sizeDelta = new Vector2(400f, 70f);
        }

        private void OnScoreChanged(int score)
        {
            _scoreText.text = score.ToString();
            _animTimer = 0f;
        }

        private void OnComboHit(int combo, Vector3 worldPos)
        {
            SpawnComboPopup(combo, worldPos);
        }

        private void OnComboBreak()
        {
            // Spawn COMBO BREAK text at screen center in world space
            Camera cam = Camera.main;
            if (cam == null) return;

            Vector3 center = cam.transform.position;
            center.z = 0f;

            var go = new GameObject("ComboBreak");
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = "COMBO BREAK";
            tmp.fontSize = 5;
            tmp.color = new Color(1f, 0.3f, 0.3f, 1f);
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.sortingOrder = 25;

            go.transform.position = center;

            var popup = go.AddComponent<ComboPopup>();
            popup.Initialize(Vector3.up);
        }

        private void SpawnComboPopup(int combo, Vector3 worldPos)
        {
            var go = new GameObject("ComboPopup");
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = "Combo " + combo;
            tmp.fontSize = 4;
            tmp.color = new Color(1f, 0.2f, 0.2f, 1f);
            tmp.outlineWidth = 0.3f;
            tmp.outlineColor = new Color(1f, 0.84f, 0f, 1f);
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.sortingOrder = 20;

            // Random angle within ±60° from straight up
            float angleDeg = Random.Range(-60f, 60f);
            float rad = (90f + angleDeg) * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);

            go.transform.position = worldPos;

            var popup = go.AddComponent<ComboPopup>();
            popup.Initialize(dir);
        }

        private void Update()
        {
            if (_animTimer >= 0f)
            {
                _animTimer += Time.deltaTime;
                float t = _animTimer / 0.2f;
                if (t >= 1f)
                {
                    _animTimer = -1f;
                    _scoreText.transform.localScale = Vector3.one;
                    _scoreText.color = Color.white;
                }
                else
                {
                    float scale = 1f + 0.15f * Mathf.Sin(t * Mathf.PI);
                    _scoreText.transform.localScale = Vector3.one * scale;
                    _scoreText.color = Color.Lerp(Color.yellow, Color.white, t);
                }
            }
        }
    }
}