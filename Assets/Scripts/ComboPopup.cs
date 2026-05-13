using UnityEngine;
using TMPro;

namespace SliceShoot.UI
{
    public class ComboPopup : MonoBehaviour
    {
        private Vector3 _moveDir;
        private float _timer;
        private TMP_Text _text;
        private Color _startColor;
        private const float DURATION = 0.5f;
        private const float MOVE_SPEED = 2f;

        public void Initialize(Vector3 direction)
        {
            _moveDir = direction;
            _text = GetComponent<TMP_Text>();
            _startColor = _text.color;
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            transform.position += _moveDir * MOVE_SPEED * Time.deltaTime;

            float t = _timer / DURATION;
            if (t >= 1f)
            {
                Destroy(gameObject);
                return;
            }

            Color c = _startColor;
            c.a = 1f - t;
            _text.color = c;

            float scale = 1f - t * 0.5f;
            transform.localScale = Vector3.one * scale;
        }
    }
}