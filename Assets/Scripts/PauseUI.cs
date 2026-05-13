using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SliceShoot.Core;

namespace SliceShoot.UI
{
    public class PauseUI : MonoBehaviour
    {
        private Canvas _buttonCanvas;
        private GameObject _buttonCanvasGO;
        private Canvas _overlayCanvas;
        private GameObject _overlayCanvasGO;
        private GameObject _pauseButtonGO;

        private void Start()
        {
            CreatePauseButton();
            CreatePauseOverlay();
            _overlayCanvasGO.SetActive(false);
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
                _pauseButtonGO.SetActive(false);
                _overlayCanvasGO.SetActive(false);
            }
            else if (state == GameState.Playing)
            {
                _pauseButtonGO.SetActive(true);
                _overlayCanvasGO.SetActive(false);
            }
            else if (state == GameState.Paused)
            {
                _overlayCanvasGO.SetActive(true);
            }
        }

        private void CreatePauseButton()
        {
            _buttonCanvasGO = new GameObject("PauseButtonCanvas");
            _buttonCanvasGO.transform.SetParent(transform);
            _buttonCanvas = _buttonCanvasGO.AddComponent<Canvas>();
            _buttonCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _buttonCanvas.sortingOrder = 100;

            var scaler = _buttonCanvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            _buttonCanvasGO.AddComponent<GraphicRaycaster>();

            // Pause button
            _pauseButtonGO = new GameObject("PauseButton");
            _pauseButtonGO.transform.SetParent(_buttonCanvasGO.transform, false);

            var btnImage = _pauseButtonGO.AddComponent<Image>();
            btnImage.color = new Color(0f, 0f, 0f, 0.5f);

            var btnRect = _pauseButtonGO.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(1f, 1f);
            btnRect.anchorMax = new Vector2(1f, 1f);
            btnRect.pivot = new Vector2(1f, 1f);
            btnRect.anchoredPosition = new Vector2(-20f, -20f);
            btnRect.sizeDelta = new Vector2(100f, 100f);

            var btn = _pauseButtonGO.AddComponent<Button>();
            btn.targetGraphic = btnImage;

            var colors = btn.colors;
            colors.normalColor = new Color(0f, 0f, 0f, 0.5f);
            colors.highlightedColor = new Color(0f, 0f, 0f, 0.7f);
            colors.pressedColor = new Color(0f, 0f, 0f, 0.8f);
            btn.colors = colors;

            // Pause icon "||"
            var iconGO = new GameObject("PauseIcon");
            iconGO.transform.SetParent(_pauseButtonGO.transform, false);
            var iconText = iconGO.AddComponent<TextMeshProUGUI>();
            iconText.text = "||";
            iconText.fontSize = 36;
            iconText.color = Color.white;
            iconText.alignment = TextAlignmentOptions.Center;
            iconText.fontStyle = FontStyles.Bold;
            var iconRect = iconText.rectTransform;
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            btn.onClick.AddListener(OnPauseClicked);
        }

        private void CreatePauseOverlay()
        {
            _overlayCanvasGO = new GameObject("PauseOverlayCanvas");
            _overlayCanvasGO.transform.SetParent(transform);
            _overlayCanvas = _overlayCanvasGO.AddComponent<Canvas>();
            _overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _overlayCanvas.sortingOrder = 200;

            var scaler = _overlayCanvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            _overlayCanvasGO.AddComponent<GraphicRaycaster>();

            // Dark overlay
            var overlayBG = new GameObject("DarkOverlay");
            overlayBG.transform.SetParent(_overlayCanvasGO.transform, false);
            var bgImage = overlayBG.AddComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0.5f);
            var bgRect = bgImage.rectTransform;
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // PAUSED text
            var pausedGO = new GameObject("PausedText");
            pausedGO.transform.SetParent(_overlayCanvasGO.transform, false);
            var pausedText = pausedGO.AddComponent<TextMeshProUGUI>();
            pausedText.text = "PAUSED";
            pausedText.fontSize = 60;
            pausedText.color = Color.white;
            pausedText.alignment = TextAlignmentOptions.Center;
            pausedText.fontStyle = FontStyles.Bold;
            var ptRect = pausedText.rectTransform;
            ptRect.anchorMin = new Vector2(0.5f, 0.6f);
            ptRect.anchorMax = new Vector2(0.5f, 0.6f);
            ptRect.pivot = new Vector2(0.5f, 0.5f);
            ptRect.anchoredPosition = Vector2.zero;
            ptRect.sizeDelta = new Vector2(600f, 100f);

            // Resume button
            var resumeGO = new GameObject("ResumeButton");
            resumeGO.transform.SetParent(_overlayCanvasGO.transform, false);

            var resumeImage = resumeGO.AddComponent<Image>();
            resumeImage.color = new Color(0.2f, 0.7f, 0.3f, 1f);

            var resumeRect = resumeGO.GetComponent<RectTransform>();
            resumeRect.anchorMin = new Vector2(0.5f, 0.4f);
            resumeRect.anchorMax = new Vector2(0.5f, 0.4f);
            resumeRect.pivot = new Vector2(0.5f, 0.5f);
            resumeRect.anchoredPosition = Vector2.zero;
            resumeRect.sizeDelta = new Vector2(260f, 80f);

            var resumeBtn = resumeGO.AddComponent<Button>();
            resumeBtn.targetGraphic = resumeImage;
            var rColors = resumeBtn.colors;
            rColors.normalColor = new Color(0.2f, 0.7f, 0.3f, 1f);
            rColors.highlightedColor = new Color(0.25f, 0.8f, 0.35f, 1f);
            rColors.pressedColor = new Color(0.15f, 0.55f, 0.25f, 1f);
            resumeBtn.colors = rColors;

            var resumeLabelGO = new GameObject("Label");
            resumeLabelGO.transform.SetParent(resumeGO.transform, false);
            var resumeLabel = resumeLabelGO.AddComponent<TextMeshProUGUI>();
            resumeLabel.text = "RESUME";
            resumeLabel.fontSize = 28;
            resumeLabel.color = Color.white;
            resumeLabel.alignment = TextAlignmentOptions.Center;
            resumeLabel.fontStyle = FontStyles.Bold;
            var rlRect = resumeLabel.rectTransform;
            rlRect.anchorMin = Vector2.zero;
            rlRect.anchorMax = Vector2.one;
            rlRect.offsetMin = Vector2.zero;
            rlRect.offsetMax = Vector2.zero;

            resumeBtn.onClick.AddListener(OnResumeClicked);
        }

        private void OnPauseClicked()
        {
            GameManager.Instance.PauseGame();
        }

        private void OnResumeClicked()
        {
            GameManager.Instance.ResumeGame();
        }
    }
}
