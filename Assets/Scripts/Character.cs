using UnityEngine;

namespace SliceShoot.Character
{
    public class Character : MonoBehaviour
    {
        public static Character Instance { get; private set; }
        public bool InputEnabled { get; set; } = true;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void FireProjectile(Vector3 worldDirection, bool enhanced)
        {
            var go = new GameObject("Projectile");
            go.transform.position = transform.position;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetCircleSprite();
            sr.color = SliceShoot.Monster.HitTypeIndicator.HitTypeColors[3]; // Projectile blue
            float size = enhanced ? 0.45f : 0.3f;
            go.transform.localScale = Vector3.one * size;
            sr.sortingOrder = 10;

            go.AddComponent<SliceShoot.Combat.Projectile>().Launch(worldDirection, enhanced);
        }

        private static Sprite _cachedCircle;
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
    }
}
