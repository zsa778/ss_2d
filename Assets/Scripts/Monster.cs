using System;
using System.Collections;
using UnityEngine;

namespace SliceShoot.Monster
{
    public class Monster : MonoBehaviour
    {
        public event Action<Monster> OnDied;
        public event Action<byte, Vector3> OnHit;
        public MonsterData Data { get; private set; }

        [SerializeField] private MonsterData _startData;
        [SerializeField] private float _hitRadius = 0.5f;

        private Animator _animator;
        private float _speed;
        private Vector3 _screenCenter;
        private float _facingSign = 1f;
        private bool _frozen;
        private float _freezeTimer;
        private bool _stunned;
        private float _stunTimer;
        private byte _currentHitIndex;
        private HitTypeIndicator _indicator;

        public float HitRadius => _hitRadius;
        public byte CurrentHitType
        {
            get
            {
                if (Data?.hitTypes == null || _currentHitIndex >= Data.hitTypes.Length) return 255;
                return Data.hitTypes[_currentHitIndex];
            }
        }

        private void Start()
        {
            if (_startData != null && Data == null)
                Initialize(_startData);
        }

        public void Initialize(MonsterData data)
        {
            Data = data;
            _speed = data.speed;
            _currentHitIndex = 0;
            _animator = GetComponent<Animator>();

            _indicator = GetComponent<HitTypeIndicator>();
            if (_indicator == null) _indicator = gameObject.AddComponent<HitTypeIndicator>();
            if (data.hitTypes != null && data.hitTypes.Length > 0)
                _indicator.Refresh(data.hitTypes, _currentHitIndex);

            var cam = Camera.main;
            if (cam != null)
            {
                Vector3 c = cam.ScreenToWorldPoint(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, -cam.transform.position.z));
                _screenCenter = new Vector3(c.x, c.y, 0f);
            }
        }

        private void Update()
        {
            if (_frozen) { _freezeTimer -= Time.deltaTime; if (_freezeTimer <= 0) _frozen = false; }
            if (_stunned) { _stunTimer -= Time.deltaTime; if (_stunTimer <= 0) _stunned = false; }

            if (!_frozen && !_stunned && _speed > 0)
            {
                Vector3 dir = _screenCenter - transform.position;
                if (Mathf.Abs(dir.x) > 0.001f)
                {
                    float sign = dir.x < 0 ? -1f : 1f;
                    if (sign != _facingSign)
                    {
                        _facingSign = sign;
                        Vector3 s = transform.localScale;
                        transform.localScale = new Vector3(Mathf.Abs(s.x) * _facingSign, s.y, s.z);
                    }
                }
                transform.position = Vector3.MoveTowards(transform.position, _screenCenter, _speed * Time.deltaTime);
            }
        }

        public bool TryHit(byte hitType, Vector3 hitPosition)
        {
            if (Data == null) return false;
            if (Data.hitTypes == null || Data.hitTypes.Length == 0) { Die(); return true; }
            if (_currentHitIndex >= Data.hitTypes.Length) return false;
            if (Data.hitTypes[_currentHitIndex] != hitType) return false;

            OnHit?.Invoke(hitType, hitPosition);
            _currentHitIndex++;

            if (_currentHitIndex >= Data.hitTypes.Length)
            {
                _indicator?.Refresh(Data.hitTypes, _currentHitIndex);
                Die();
            }
            else
            {
                _indicator?.RefreshAnimated(Data.hitTypes, _currentHitIndex);
            }
            return true;
        }

        public void Die()
        {
            OnDied?.Invoke(this);
            Destroy(gameObject);
        }

        public void FreezePosition(float duration)
        {
            _frozen = true;
            _freezeTimer = Mathf.Max(_freezeTimer, duration);
        }

        public void Stun(float duration)
        {
            _stunned = true;
            _stunTimer = Mathf.Max(_stunTimer, duration);
        }

        public void Knockback(Vector3 direction, float distance, float duration)
        {
            StartCoroutine(KnockbackCoroutine(direction.normalized * distance, duration));
        }

        private IEnumerator KnockbackCoroutine(Vector3 offset, float duration)
        {
            FreezePosition(duration);
            Vector3 start = transform.position;
            Vector3 end = start + offset;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                if (this == null) yield break;
                float t = elapsed / duration;
                t = 1f - (1f - t) * (1f - t);
                transform.position = Vector3.Lerp(start, end, t);
                yield return null;
            }
            if (this != null) transform.position = end;
        }

        public void SetAnimSpeed(float speed)
        {
            if (_animator != null) _animator.speed = speed;
        }

        public void SetCombatVisualsVisible(bool visible) { }

        public void TriggerSpawnAnimation(string trigger)
        {
            if (_animator != null && !string.IsNullOrEmpty(trigger))
                _animator.SetTrigger(trigger);
        }
    }
}
