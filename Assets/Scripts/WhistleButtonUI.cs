using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using SliceShoot.Core;

namespace SliceShoot.UI
{
    public class WhistleButtonUI : MonoBehaviour
    {
        private const float COOLDOWN_DURATION = 30f;
        private const float STUN_DURATION = 2f;

        public bool IsOnCooldown { get { return _onCooldown; } }
        public float CooldownRemaining { get { return _cooldownTimer; } }

        [SerializeField] private Button _whistleButton;

        private bool _onCooldown;
        private float _cooldownTimer;

        private Image _image;
        private RectTransform _rectTransform;

        private void Start()
        {
            if (_whistleButton == null)
                _whistleButton = GetComponentInChildren<Button>(true);
            if (_whistleButton != null)
                _whistleButton.onClick.AddListener(OnWhistleClicked);

            _image = _whistleButton.GetComponent<Image>();
            _rectTransform = _whistleButton.GetComponent<RectTransform>();
        }

        private bool IsHitOnSprite()
        {
            if (Pointer.current == null) return false;
            Vector2 inputPos = Pointer.current.position.ReadValue();

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rectTransform, inputPos, null, out Vector2 local))
                return false;

            var sprite = _image.sprite;
            var tex = sprite.texture;
            var spriteRect = sprite.textureRect;
            var rect = _rectTransform.rect;

            int x = Mathf.Clamp((int)(spriteRect.x + (local.x - rect.x) / rect.width * spriteRect.width), 0, tex.width - 1);
            int y = Mathf.Clamp((int)(spriteRect.y + (local.y - rect.y) / rect.height * spriteRect.height), 0, tex.height - 1);

            return tex.GetPixel(x, y).a >= 0.1f;
        }

        private void OnDestroy()
        {
            if (_whistleButton != null)
                _whistleButton.onClick.RemoveListener(OnWhistleClicked);
        }

        private void Update()
        {
            if (_onCooldown)
            {
                _cooldownTimer -= Time.unscaledDeltaTime;
                if (_cooldownTimer <= 0f)
                {
                    _onCooldown = false;
                    _cooldownTimer = 0f;
                }
            }
        }

        public void OnWhistleClicked()
        {
            if (!IsHitOnSprite()) return;
            if (_onCooldown) return;
            if (GameManager.Instance.CurrentState != GameState.Playing) return;

            if (SliceShoot.Monster.MonsterSpawner.Instance != null)
                SliceShoot.Monster.MonsterSpawner.Instance.StunAllMonsters(STUN_DURATION);

            StartCoroutine(CreateShockwaveVFX());

            _onCooldown = true;
            _cooldownTimer = COOLDOWN_DURATION;
        }

        private IEnumerator CreateShockwaveVFX()
        {
            var waveGO = new GameObject("WhistleShockwave");
            waveGO.transform.position = Vector3.zero;

            var lr = waveGO.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.loop = true;
            lr.startWidth = 0.08f;
            lr.endWidth = 0.08f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = new Color(1f, 1f, 0f, 0.7f);
            lr.endColor = new Color(1f, 1f, 0f, 0.7f);
            lr.sortingLayerName = "Monster";
            lr.sortingOrder = 20;

            int segments = 64;
            lr.positionCount = segments;

            Camera cam = Camera.main;
            float maxRadius = cam != null ? cam.orthographicSize * 1.5f : 10f;
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float radius = Mathf.Lerp(0f, maxRadius, t);
                float alpha = Mathf.Lerp(0.7f, 0f, t);

                lr.startColor = new Color(1f, 1f, 0f, alpha);
                lr.endColor = new Color(1f, 1f, 0f, alpha);

                for (int i = 0; i < segments; i++)
                {
                    float angle = (float)i / segments * 360f * Mathf.Deg2Rad;
                    lr.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
                }

                yield return null;
            }

            Destroy(waveGO);
        }
    }
}
