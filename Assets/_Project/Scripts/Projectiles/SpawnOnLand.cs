using UnityEngine;

// Add next to an ArcProjectile: when it lands, spawn a prefab at the landing point (a crater, a fire patch...).
namespace _Project.Scripts.Projectiles
{
    [RequireComponent(typeof(ArcProjectile))]
    public class SpawnOnLand : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;

        [Tooltip("Seconds until the spawned object removes itself. 0 = it stays forever.")]
        [SerializeField, Min(0f)] private float lifetime = 10f;

        private ArcProjectile _projectile;

        private void Awake()
        {
            _projectile = GetComponent<ArcProjectile>();
        }

        private void OnEnable()
        {
            _projectile.Landed += OnLanded;
        }

        private void OnDisable()
        {
            if (_projectile != null)
            {
                _projectile.Landed -= OnLanded;
            }
        }

        private void OnLanded(Vector3 point)
        {
            if (prefab == null)
            {
                return;
            }

            GameObject spawned = Instantiate(prefab, point, Quaternion.identity);

            if (lifetime > 0f)
            {
                Destroy(spawned, lifetime);
            }
        }
    }
}