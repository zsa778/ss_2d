using System.Collections;
using UnityEngine;

namespace SliceShoot.Core
{
    public class CameraAnimator : MonoBehaviour
    {
        public Vector3 OriginalPos { get; private set; }
        public float OriginalSize { get; private set; }

        private Camera _cam;

        private void Awake()
        {
            _cam = Camera.main;
            OriginalPos = _cam.transform.position;
            OriginalSize = _cam.orthographicSize;
        }

        public IEnumerator LerpTo(Vector3 targetWorldPos, float targetSize, float duration)
        {
            Vector3 startPos = _cam.transform.position;
            float startSize = _cam.orthographicSize;
            Vector3 endPos = new Vector3(targetWorldPos.x, targetWorldPos.y, startPos.z);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                _cam.transform.position = Vector3.Lerp(startPos, endPos, t);
                _cam.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
                yield return null;
            }
            _cam.transform.position = endPos;
            _cam.orthographicSize = targetSize;
        }
    }
}
