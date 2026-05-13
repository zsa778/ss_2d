using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SliceShoot.Core;

namespace SliceShoot.UI
{
    public class GameOverUI : MonoBehaviour
    {
        private Canvas _canvas;
        private GameObject _canvasGO;
        private Image _overlay;
        private TextMeshProUGUI _gameOverText;
        private TextMeshProUGUI _finalScoreText;
        private TextMeshProUGUI _bestScoreText;
        private GameObject _buttonContainer;
        private Button _restartButton;
        private Button _menuButton;

        private void Start()
        {
            CreateUI();
            _canvasGO.SetActive(false);
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.GameOver)
            {
                _canvasGO.SetActive(true);
                StartCoroutine(PlayGameOverSequence());
            }
        }

        private void CreateUI()
        {
            _canvasGO = new GameObject("GameOverCanvas");
            _canvasGO.transform.SetParent(transform);
            _canvas = _canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 300;

            var scaler = _canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            _canvasGO.AddComponent<GraphicRaycaster>();

            // Dark overlay
            var overlayGO = new GameObject("DarkOverlay");
            overlayGO.transform.SetParent(_canvasGO.transform, false);
            _overlay = overlayGO.AddComponent<Image>();
            _overlay.color = new Color(0f, 0f, 0f, 0f);
            var overlayRect = _overlay.rectTransform;
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            // GAME OVER text
            var gameOverGO = new GameObject("GameOverText");
            gameOverGO.transform.SetParent(_canvasGO.transform, false);
            _gameOverText = gameOverGO.AddComponent<TextMeshProUGUI>();
            _gameOverText.text = "GAME OVER";
            _gameOverText.fontSize = 72;
            _gameOverText.color = new Color(1f, 0.2f, 0.2f, 0f);
            _gameOverText.alignment = TextAlignmentOptions.Center;
            _gameOverText.fontStyle = FontStyles.Bold;
            _gameOverText.outlineWidth = 0.15f;
            _gameOverText.outlineColor = new Color(0.3f, 0f, 0f, 1f);
            var goRect = _gameOverText.rectTransform;
            goRect.anchorMin = new Vector2(0.5f, 0.7f);
            goRect.anchorMax = new Vector2(0.5f, 0.7f);
            goRect.pivot = new Vector2(0.5f, 0.5f);
            goRect.anchoredPosition = new Vector2(0f, 300f);
            goRect.sizeDelta = new Vector2(800f, 120f);

            // FINAL SCORE text
            var scoreGO = new GameObject("FinalScoreText");
            scoreGO.transform.SetParent(_canvasGO.transform, false);
            _finalScoreText = scoreGO.AddComponent<TextMeshProUGUI>();
            _finalScoreText.text = "";
            _finalScoreText.fontSize = 48;
            _finalScoreText.color = new Color(1f, 1f, 1f, 0f);
            _finalScoreText.alignment = TextAlignmentOptions.Center;
            _finalScoreText.fontStyle = FontStyles.Bold;
            var fsRect = _finalScoreText.rectTransform;
            fsRect.anchorMin = new Vector2(0.5f, 0.5f);
            fsRect.anchorMax = new Vector2(0.5f, 0.5f);
            fsRect.pivot = new Vector2(0.5f, 0.5f);
            fsRect.anchoredPosition = Vector2.zero;
            fsRect.sizeDelta = new Vector2(800f, 80f);

            // BEST SCORE text
            var bestGO = new GameObject("BestScoreText");
            bestGO.transform.SetParent(_canvasGO.transform, false);
            _bestScoreText = bestGO.AddComponent<TextMeshProUGUI>();
            _bestScoreText.text = "";
            _bestScoreText.fontSize = 32;
            _bestScoreText.color = new Color(1f, 0.84f, 0f, 0f);
            _bestScoreText.alignment = TextAlignmentOptions.Center;
            _bestScoreText.fontStyle = FontStyles.Bold;
            var bsRect = _bestScoreText.rectTransform;
            bsRect.anchorMin = new Vector2(0.5f, 0.4f);
            bsRect.anchorMax = new Vector2(0.5f, 0.4f);
            bsRect.pivot = new Vector2(0.5f, 0.5f);
            bsRect.anchoredPosition = Vector2.zero;
            bsRect.sizeDelta = new Vector2(800f, 60f);

            // Buttons - manually positioned instead of using HorizontalLayoutGroup
            _restartButton = CreateButton(_canvasGO.transform, "RESTART",
                new Color(0.2f, 0.7f, 0.3f, 1f), new Vector2(-140f, 0f), OnRestartClicked);

            _menuButton = CreateButton(_canvasGO.transform, "MENU",
                new Color(0.3f, 0.5f, 0.9f, 1f), new Vector2(140f, 0f), OnMenuClicked);

            // Button container for tracking
            _buttonContainer = new GameObject("ButtonContainer");
            _buttonContainer.transform.SetParent(_canvasGO.transform, false);
            _restartButton.transform.SetParent(_buttonContainer.transform, true);
            _menuButton.transform.SetParent(_buttonContainer.transform, true);
            _buttonContainer.SetActive(false);
        }

        private Button CreateButton(Transform parent, string label, Color bgColor, Vector2 offset, UnityEngine.Events.UnityAction onClick)
        {
            var btnGO = new GameObject(label + "Button");
            btnGO.transform.SetParent(parent, false);

            var btnImage = btnGO.AddComponent<Image>();
            btnImage.color = bgColor;

            var btnRect = btnGO.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.2f);
            btnRect.anchorMax = new Vector2(0.5f, 0.2f);
            btnRect.pivot = new Vector2(0.5f, 0.5f);
            btnRect.anchoredPosition = offset;
            btnRect.sizeDelta = new Vector2(220f, 80f);

            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnImage;

            var colors = btn.colors;
            colors.normalColor = bgColor;
            colors.highlightedColor = new Color(
                Mathf.Min(bgColor.r * 1.2f, 1f),
                Mathf.Min(bgColor.g * 1.2f, 1f),
                Mathf.Min(bgColor.b * 1.2f, 1f), 1f);
            colors.pressedColor = new Color(bgColor.r * 0.8f, bgColor.g * 0.8f, bgColor.b * 0.8f, 1f);
            btn.colors = colors;

            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(btnGO.transform, false);
            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 28;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            var labelRect = tmp.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            btn.onClick.AddListener(onClick);
            return btn;
        }

        private IEnumerator PlayGameOverSequence()
        {
            float elapsed = 0f;

            // Phase 1: Overlay fade-in (0.5s)
            elapsed = 0f;
            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / 0.5f);
                _overlay.color = new Color(0f, 0f, 0f, t * 0.7f);
                yield return null;
            }
            _overlay.color = new Color(0f, 0f, 0f, 0.7f);

            // Phase 2: GAME OVER text bounce drop (0.5s)
            float startY = 300f;
            float targetY = 0f;
            var goRect = _gameOverText.rectTransform;
            elapsed = 0f;
            _gameOverText.color = new Color(1f, 0.2f, 0.2f, 1f);
            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / 0.5f);
                float y;
                if (t < 0.6f)
                {
                    float bt = t / 0.6f;
                    y = Mathf.Lerp(startY, targetY, bt * bt);
                }
                else if (t < 0.8f)
                {
                    float bt = (t - 0.6f) / 0.2f;
                    y = targetY - 40f * Mathf.Sin(bt * Mathf.PI);
                }
                else
                {
                    float bt = (t - 0.8f) / 0.2f;
                    y = targetY - 15f * Mathf.Sin(bt * Mathf.PI);
                }
                goRect.anchoredPosition = new Vector2(0f, y);
                yield return null;
            }
            goRect.anchoredPosition = new Vector2(0f, targetY);

            // Phase 3: Score counting (1s)
            int finalScore = 0;
            if (ScoreManager.Instance != null) finalScore = ScoreManager.Instance.Score;
            elapsed = 0f;
            _finalScoreText.color = Color.white;
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / 1f);
                int displayScore = Mathf.RoundToInt(Mathf.Lerp(0f, (float)finalScore, t));
                _finalScoreText.text = "FINAL SCORE: " + displayScore;
                yield return null;
            }
            _finalScoreText.text = "FINAL SCORE: " + finalScore;

            // Phase 4: Best score
            int bestScore = PlayerPrefs.GetInt("BestScore", 0);
            bool isNewRecord = finalScore > bestScore;
            if (isNewRecord)
            {
                PlayerPrefs.SetInt("BestScore", finalScore);
                PlayerPrefs.Save();
                bestScore = finalScore;
            }
            _bestScoreText.text = "BEST: " + bestScore;
            _bestScoreText.color = new Color(1f, 0.84f, 0f, 1f);

            if (isNewRecord)
            {
                StartCoroutine(SparkleText(_bestScoreText, 2f));
            }

            yield return new WaitForSeconds(0.2f);

            // Phase 5: Show buttons with fade
            _buttonContainer.SetActive(true);

            var buttonImages = _buttonContainer.GetComponentsInChildren<Image>();
            var buttonTexts = _buttonContainer.GetComponentsInChildren<TextMeshProUGUI>();

            foreach (var img in buttonImages)
            {
                Color c = img.color; c.a = 0f; img.color = c;
            }
            foreach (var txt in buttonTexts)
            {
                Color c = txt.color; c.a = 0f; txt.color = c;
            }

            elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / 0.3f);
                foreach (var img in buttonImages)
                {
                    Color c = img.color; c.a = t; img.color = c;
                }
                foreach (var txt in buttonTexts)
                {
                    Color c = txt.color; c.a = t; txt.color = c;
                }
                yield return null;
            }
        }

        private IEnumerator SparkleText(TextMeshProUGUI text, float duration)
        {
            float elapsed = 0f;
            Color gold = new Color(1f, 0.84f, 0f, 1f);
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.PingPong(elapsed * 5f, 1f);
                text.color = Color.Lerp(gold, Color.white, t);
                yield return null;
            }
            text.color = gold;
        }

        private void OnRestartClicked()
        {
            GameManager.Instance.RestartGame();
        }

        private void OnMenuClicked()
        {
            GameManager.Instance.RestartGame();
        }
    }
}
