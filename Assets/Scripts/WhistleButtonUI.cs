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
        [SerializeField] private ShockwaveEffect _shockwaveEffect;

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

            if (_shockwaveEffect == null)
                _shockwaveEffect = GetComponent<ShockwaveEffect>();
            if (_shockwaveEffect == null)
                _shockwaveEffect = gameObject.AddComponent<ShockwaveEffect>();
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

            _shockwaveEffect?.Play();

            _onCooldown = true;
            _cooldownTimer = COOLDOWN_DURATION;
        }

    }
}
