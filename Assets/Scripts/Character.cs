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
            SliceShoot.Combat.Projectile.Create(transform.position, worldDirection, enhanced);
        }
    }
}
