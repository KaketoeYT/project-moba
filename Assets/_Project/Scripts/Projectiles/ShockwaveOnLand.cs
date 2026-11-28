using System.Collections.Generic;
using _Project.Scripts.Combat;
using _Project.Scripts.FlatPhysics;
using _Project.Scripts.Player;
using UnityEngine;

// Add next to an ArcProjectile: when it lands, damage and push everything nearby away from the impact point.
namespace _Project.Scripts.Projectiles
{
    [RequireComponent(typeof(ArcProjectile))]
    public class ShockwaveOnLand : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float radius = 4f;

        [Tooltip("Push speed right at the centre, fading to 0 at the edge. A push travels roughly (push speed / the target's drag) units.")]
        [SerializeField, Min(0f)] private float pushSpeed = 18f;

        [SerializeField, Min(0f)] private float damage = 1f;

        [Tooltip("Also push and hurt whoever fired the projectile")]
        [SerializeField] private bool affectOwner;

        [SerializeField] private LayerMask affectedLayers = ~0;

        // Reused for every shockwave instead of allocating a new list each time
        private static readonly List<FlatCollider> Hits = new List<FlatCollider>();

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
            Vector2 flatPoint = FlatWorld.ToFlat(point);
            FlatWorld.OverlapCircle(flatPoint, radius, Hits, affectedLayers);
            GameObject owner = _projectile.Owner;

            foreach (FlatCollider hit in Hits)
            {
                if (!affectOwner && owner != null && hit.transform.IsChildOf(owner.transform))
                {
                    continue;
                }

                if (hit.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(damage);
                }

                if (hit.TryGetComponent(out IShockwaveReceiver reactive))
                {
                    // Measured to the nearest edge, so big things like walls react fairly
                    float surfaceDistance = Vector2.Distance(hit.ClosestPoint(flatPoint), flatPoint);
                    reactive.OnShockwave(point, 1f - Mathf.Clamp01(surfaceDistance / radius));
                }

                if (hit.TryGetComponent(out ForceReceiver pushReceiver))
                {
                    Vector3 away = hit.transform.position - point;
                    away.y = 0f;
                    float distance = away.magnitude;

                    // Standing exactly on the centre: there is no "away", so pick a direction
                    Vector3 direction = distance > 0.001f ? away / distance : Vector3.forward;
                    float strength = 1f - Mathf.Clamp01(distance / radius);

                    pushReceiver.AddImpulse(direction * (pushSpeed * strength));
                }
            }
        }
    }
}