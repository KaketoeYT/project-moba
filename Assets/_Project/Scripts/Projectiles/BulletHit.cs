using System.Collections.Generic;
using _Project.Scripts.Combat;
using _Project.Scripts.FlatPhysics;
using UnityEngine;

// Damages and removes the bullet on the first solid thing it touches (not zones, not its owner).
// Checks the whole path it moved this frame, so fast bullets can't skip through thin walls.
namespace _Project.Scripts.Projectiles
{
    [RequireComponent(typeof(FlatCollider))]
    public class BulletHit : MonoBehaviour
    {
        [SerializeField] private BulletVar bulletVar;

        // Shared by every bullet: only used inside one call
        private static readonly List<FlatCollider> Hits = new List<FlatCollider>();

        private FlatCollider _body;
        private Vector2 _lastCenter;
        private bool _hasHit;

        private void Awake()
        {
            _body = GetComponent<FlatCollider>();
        }

        private void Start()
        {
            _lastCenter = _body.Center;
        }

        // LateUpdate: after BulletMove has moved the bullet this frame
        private void LateUpdate()
        {
            if (_hasHit)
            {
                return;
            }

            Vector2 center = _body.Center;
            float radius = _body.BoundingRadius;

            // Check points along the path, no further apart than the bullet's radius
            float distance = Vector2.Distance(_lastCenter, center);
            int steps = Mathf.Clamp(Mathf.CeilToInt(distance / Mathf.Max(radius, 0.05f)), 1, 32);

            for (int i = 1; i <= steps; i++)
            {
                Vector2 point = Vector2.Lerp(_lastCenter, center, (float)i / steps);
                FlatWorld.OverlapCircle(point, radius, Hits);

                foreach (FlatCollider other in Hits)
                {
                    // Ignore whoever fired this bullet, including their child colliders
                    if (bulletVar.bulletOwner != null && other.transform.IsChildOf(bulletVar.bulletOwner.transform))
                    {
                        continue;
                    }

                    Hit(other);
                    return;
                }
            }

            _lastCenter = center;
        }

        private void Hit(FlatCollider other)
        {
            _hasHit = true;

            // Look on this object and its parents, so things with several child colliders still count
            IDamageable damageable = other.GetComponentInParent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(bulletVar.bulletDamage);
            }

            Destroy(gameObject);
        }
    }
}
