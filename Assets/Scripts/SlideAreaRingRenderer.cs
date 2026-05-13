using UnityEngine;
using SliceShoot.Monster;

namespace SliceShoot.Character
{
    [ExecuteAlways]
    [RequireComponent(typeof(LineRenderer))]
    public class SlideAreaRingRenderer : MonoBehaviour
    {
        [SerializeField] private float _editorRadius = 1.75f;

        private LineRenderer _lr;
        private float _lastRadius = -1f;
        private const int SEGMENTS = 64;

        private void Awake()
        {
            SetupLineRenderer();
            BuildCircle(GetCurrentRadius());
        }

        private void OnValidate()
        {
            if (_lr == null) _lr = GetComponent<LineRenderer>();
            if (_lr != null) BuildCircle(_editorRadius);
        }

        private void SetupLineRenderer()
        {
            _lr = GetComponent<LineRenderer>();
            _lr.loop = true;
            _lr.useWorldSpace = false;
            _lr.positionCount = SEGMENTS;
            _lr.startWidth = 0.04f;
            _lr.endWidth = 0.04f;
            _lr.startColor = new Color(1f, 1f, 1f, 0.3f);
            _lr.endColor = new Color(1f, 1f, 1f, 0.3f);
            _lr.sortingOrder = -1;
            if (_lr.sharedMaterial == null)
                _lr.material = new Material(Shader.Find("Sprites/Default"));
        }

        private void LateUpdate()
        {
            if (_lr == null) SetupLineRenderer();
            float r = GetCurrentRadius();
            if (Mathf.Abs(r - _lastRadius) < 0.001f) return;
            _lastRadius = r;
            BuildCircle(r);
        }

        private float GetCurrentRadius()
        {
            if (Application.isPlaying && MonsterSpawner.Instance != null)
            {
                float r = MonsterSpawner.Instance.GetSpawnZone1Outer();
                if (r > 0.001f) return r;
            }
            return _editorRadius;
        }

        private void BuildCircle(float r)
        {
            if (_lr == null) return;
            _lastRadius = r;
            for (int i = 0; i < SEGMENTS; i++)
            {
                float angle = (i / (float)SEGMENTS) * Mathf.PI * 2f;
                _lr.SetPosition(i, new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r, 0f));
            }
        }
    }
}
