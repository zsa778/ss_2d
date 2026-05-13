using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace SliceShoot.Monster
{
    public class HitTypeIndicator : MonoBehaviour
    {
        private const float GAP_RATIO = 0.06f;  // gap = collider/sprite height * this
        private const float ICON_SPACING = 0.25f;
        private const float CURRENT_SCALE = 0.22f;
        private const float NEXT_SCALE = 0.16f;
        private const float PULSE_SPEED = 4f;
        private const float PULSE_RANGE = 0.04f;
        private const int MAX_VISIBLE = 4;

        public static readonly Color[] HitTypeColors = new Color[] {
            new Color(1f, 0.27f, 0.27f, 1f),   // 0 Touch: Red #FF4444
            new Color(1f, 0.6f, 0f, 1f),        // 1 Slide: Orange #FF9D00
            new Color(1f, 1f, 1f, 1f),           // 2 Hold: White
            new Color(0.6f, 0.85f, 1f, 1f),        // 3 Projectile: Light Blue
            new Color(1f, 0.84f, 0f, 1f),        // 4 Area: Yellow #FFD700
        };

        private Transform _container;
        private readonly List<SpriteRenderer> _icons = new List<SpriteRenderer>();
        private TextMeshPro _overflowArrow;
        private int _visibleCount;

        private SpriteRenderer _modelSR;
        private Collider2D _collider;

        private const string UISortingLayer = "Monster";
        private const int    UISortingOrder  = 1000;

        private void Awake()
        {

            // Cache collider for accurate visual-top detection (avoids transparent sprite padding)
            _collider = GetComponent<Collider2D>();

            // Cache the root sprite: prefer SR on this GameObject, fall back to first child
            _modelSR = GetComponent<SpriteRenderer>();
            if (_modelSR == null)
            {
                foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
                {
                    if (sr.gameObject != gameObject) { _modelSR = sr; break; }
                }
            }

            // Load circle sprite for indicator icons
            Sprite circleSprite = Resources.Load<Sprite>("Circle");
            if (circleSprite == null)
                circleSprite = CreateCircleSprite();
            Material material = new Material(Shader.Find("Sprites/Default"));

            var containerGO = new GameObject("HitTypeIcons");
            containerGO.transform.SetParent(transform);
            containerGO.transform.localPosition = Vector3.zero;
            _container = containerGO.transform;

            for (int i = 0; i < MAX_VISIBLE; i++)
            {
                var iconGO = new GameObject("Icon_" + i);
                iconGO.transform.SetParent(_container);
                var sr = iconGO.AddComponent<SpriteRenderer>();
                sr.sprite = circleSprite;
                sr.sharedMaterial = material;
                sr.sortingLayerName = UISortingLayer;
                sr.sortingOrder = UISortingOrder;
                iconGO.SetActive(false);
                _icons.Add(sr);
            }

            // Overflow arrow using TextMeshPro
            var arrowGO = new GameObject("OverflowArrow");
            arrowGO.transform.SetParent(_container);
            _overflowArrow = arrowGO.AddComponent<TextMeshPro>();
            _overflowArrow.text = "<<";
            _overflowArrow.fontSize = 2;
            _overflowArrow.color = new Color(1f, 1f, 1f, 0.6f);
            _overflowArrow.alignment = TextAlignmentOptions.Center;
            _overflowArrow.rectTransform.sizeDelta = new Vector2(0.5f, 0.5f);
            var arrowRenderer = arrowGO.GetComponent<MeshRenderer>();
            arrowRenderer.sortingLayerName = UISortingLayer;
            arrowRenderer.sortingOrder = UISortingOrder;
            arrowGO.SetActive(false);
        }

        private bool _transitioning;

        public void RefreshAnimated(byte[] hitTypes, byte currentHitIndex)
        {
            if (_transitioning)
            {
                Refresh(hitTypes, currentHitIndex);
                return;
            }
            StartCoroutine(TransitionRoutine(hitTypes, currentHitIndex));
        }

        private IEnumerator TransitionRoutine(byte[] hitTypes, byte currentHitIndex)
        {
            _transitioning = true;
            if (_visibleCount > 0 && _icons[0].gameObject.activeSelf)
            {
                float elapsed = 0f;
                Color startColor = _icons[0].color;
                while (elapsed < 0.15f)
                {
                    elapsed += Time.deltaTime;
                    Color c = startColor;
                    c.a = Mathf.Lerp(1f, 0f, elapsed / 0.15f);
                    _icons[0].color = c;
                    yield return null;
                }
            }
            Refresh(hitTypes, currentHitIndex);
            _transitioning = false;
        }

        public void Refresh(byte[] hitTypes, byte currentHitIndex)
        {
            int remaining = hitTypes.Length - currentHitIndex;
            _visibleCount = Mathf.Min(remaining, MAX_VISIBLE);
            bool hasOverflow = remaining > MAX_VISIBLE;
            int totalSlots = _visibleCount + (hasOverflow ? 1 : 0);

            float totalWidth = (totalSlots > 1) ? (totalSlots - 1) * ICON_SPACING : 0f;
            float startX = -totalWidth * 0.5f;

            for (int i = 0; i < _icons.Count; i++)
            {
                if (i < _visibleCount)
                {
                    int htIdx = currentHitIndex + i;
                    int hitType = hitTypes[htIdx];

                    _icons[i].gameObject.SetActive(true);
                    if (hitType < HitTypeColors.Length)
                        _icons[i].color = HitTypeColors[hitType];

                    float scale = (i == 0) ? CURRENT_SCALE : NEXT_SCALE;
                    _icons[i].transform.localScale = new Vector3(scale, scale, 1f);
                    _icons[i].transform.localPosition = new Vector3(startX + i * ICON_SPACING, 0f, 0f);
                }
                else
                {
                    _icons[i].gameObject.SetActive(false);
                }
            }

            if (hasOverflow)
            {
                _overflowArrow.gameObject.SetActive(true);
                _overflowArrow.transform.localPosition = new Vector3(startX + _visibleCount * ICON_SPACING, 0f, 0f);
            }
            else
            {
                _overflowArrow.gameObject.SetActive(false);
            }
        }

        private static Sprite CreateCircleSprite()
        {
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size * 0.5f;
            float radius = center - 1f;
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x - center, dy = y - center;
                float alpha = Mathf.Clamp01(radius - Mathf.Sqrt(dx * dx + dy * dy) + 1f);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private float ComputeModelTopY()
        {
            if (_collider != null) return _collider.bounds.max.y;
            if (_modelSR != null) return _modelSR.bounds.max.y;
            return transform.position.y + transform.lossyScale.y * 0.5f;
        }

        private float ComputeReferenceHeight()
        {
            if (_collider != null) return _collider.bounds.size.y;
            if (_modelSR != null) return _modelSR.bounds.size.y;
            return transform.lossyScale.y;
        }

        private void LateUpdate()
        {
            if (_container == null) return;

            _container.rotation = Camera.main.transform.rotation;

            Vector3 ps = transform.lossyScale;
            float absX = Mathf.Abs(ps.x), absY = Mathf.Abs(ps.y);
            if (absX > 0.001f && absY > 0.001f)
            {
                _container.localScale = new Vector3(1f / ps.x, 1f / absY, 1f);
            }

            if (_visibleCount > 0 && _icons[0].gameObject.activeSelf)
            {
                float pulse = CURRENT_SCALE + Mathf.Sin(Time.time * PULSE_SPEED) * PULSE_RANGE;
                _icons[0].transform.localScale = new Vector3(pulse, pulse, 1f);
            }

            if (_visibleCount == 0) return;

            float modelTopY = ComputeModelTopY();
            float gap = ComputeReferenceHeight() * GAP_RATIO;
            Vector3 mpos = transform.position;
            _container.position = new Vector3(mpos.x, modelTopY + gap, mpos.z);
        }
    }
}
