using System.Collections;
using UnityEngine;

namespace SliceShoot.UI
{
    public class ShockwaveEffect : MonoBehaviour
    {
        public void Play() => StartCoroutine(RunEffect());

        private IEnumerator RunEffect()
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

            const int segments = 64;
            lr.positionCount = segments;

            Camera cam = Camera.main;
            float maxRadius = cam != null ? cam.orthographicSize * 1.5f : 10f;
            const float duration = 0.5f;
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
