using System.Collections.Generic;
using UnityEngine;
using SliceShoot.Core;
using MonsterClass = SliceShoot.Monster.Monster;

namespace SliceShoot.Combat
{
    public class Projectile : MonoBehaviour
    {
        private const float SPEED = 30f;
        private const float MAX_AGE = 3f;
        private const float COLLIDER_RADIUS = 0.15f;

        private Vector3 _velocity;
        private float _age;
        private bool _enhanced;
        private readonly HashSet<MonsterClass> _hitMonsters = new HashSet<MonsterClass>();

        private static Sprite _cachedCircle;

        public static void Create(Vector3 position, Vector3 direction, bool enhanced)
        {
            var go = new GameObject("Projectile");
            go.transform.position = position;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetCircleSprite();
            sr.color = SliceShoot.Monster.HitTypeIndicator.HitTypeColors[3];
            sr.sortingOrder = 10;
            go.transform.localScale = Vector3.one * (enhanced ? 0.45f : 0.3f);

            go.AddComponent<Projectile>().Launch(direction, enhanced);
        }

        private static Sprite GetCircleSprite()
        {
            if (_cachedCircle != null) return _cachedCircle;
            _cachedCircle = Resources.Load<Sprite>("Circle");
            if (_cachedCircle != null) return _cachedCircle;

            var tex = new Texture2D(4, 4);
            var pixels = new Color[16];
            for (int i = 0; i < 16; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            _cachedCircle = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
            return _cachedCircle;
        }

        public void Launch(Vector3 direction, bool enhanced)
        {
            _velocity = direction.normalized * SPEED;
            _enhanced = enhanced;

            var rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;

            var col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = COLLIDER_RADIUS;
        }

        private void Update()
        {
            transform.position += _velocity * Time.deltaTime;
            _age += Time.deltaTime;

            if (_age > MAX_AGE) { Destroy(gameObject); return; }

            var cam = Camera.main;
            if (cam != null)
            {
                Vector3 vp = cam.WorldToViewportPoint(transform.position);
                if (vp.x < -0.1f || vp.x > 1.1f || vp.y < -0.1f || vp.y > 1.1f)
                {
                    Destroy(gameObject);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var monster = other.GetComponent<MonsterClass>();
            if (monster == null || _hitMonsters.Contains(monster)) return;

            _hitMonsters.Add(monster);
            bool hit = monster.TryHit(3, transform.position);
            if (!hit) ScoreManager.Instance?.RegisterMiss();

            if (!_enhanced) Destroy(gameObject);
        }
    }
}
