using System.Collections;
using UnityEngine;

namespace SliceShoot.Core
{
    public class PaintManager : MonoBehaviour
    {
        public static PaintManager Instance { get; private set; }

        [SerializeField] private Sprite[] _splashSprites;
        [SerializeField] private Sprite[] _brushSprites;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (_splashSprites == null || _splashSprites.Length == 0)
                _splashSprites = Resources.LoadAll<Sprite>("SimpleSplashBlur");
            if (_brushSprites == null || _brushSprites.Length == 0)
                _brushSprites = Resources.LoadAll<Sprite>("Brush_stroke_spritesheet");
        }

        public void SpawnSplash(Transform parent, Vector3 worldPos, Color color)
            => Spawn(parent, worldPos, color, _splashSprites);

        public void SpawnBrushStroke(Transform parent, Vector3 worldPos, Color color)
            => Spawn(parent, worldPos, color, _brushSprites);

        private void Spawn(Transform parent, Vector3 worldPos, Color color, Sprite[] sprites)
        {
            if (sprites == null || sprites.Length == 0) return;

            var go = new GameObject("Paint");
            go.transform.SetParent(parent, true);
            go.transform.position = new Vector3(worldPos.x, worldPos.y, 0f);
            go.transform.localScale = Vector3.zero;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprites[Random.Range(0, sprites.Length)];
            sr.color = color;
            sr.sortingLayerName = "Paint";
            sr.sortingOrder = 0;

            StartCoroutine(ScaleIn(go.transform, 1f, 0.05f));
            StartCoroutine(ApplyMaskNextFrame(sr));
        }

        private IEnumerator ScaleIn(Transform t, float target, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (t == null) yield break;
                elapsed += Time.deltaTime;
                t.localScale = Vector3.one * Mathf.Lerp(0f, target, elapsed / duration);
                yield return null;
            }
            if (t != null) t.localScale = Vector3.one * target;
        }

        private IEnumerator ApplyMaskNextFrame(SpriteRenderer sr)
        {
            yield return null;
            if (sr != null)
                sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }
    }
}
