using System.Collections.Generic;
using _Project.Scripts.FlatPhysics;
using _Project.Scripts.Player;
using _Project.Scripts.Utilities;
using UnityEngine;

namespace _Project.Scripts.Projectiles
{
    public class BulletHoming : MonoBehaviour
    {
        [SerializeField] private BulletVar bulletVar;

        [Tooltip("How fast the bullet can turn towards its target, in degrees per second")]
        [SerializeField, Min(0f)] private float turnSpeed = 360f;

        // Shared by every bullet: only used inside one call
        private static readonly List<FlatCollider> InZone = new List<FlatCollider>();

        private Transform _target;

        // Optional detection zone: a FlatCollider set to Is Trigger on this same object
        private FlatCollider _zone;

        // Called by spawners that already know their target (e.g. towers).
        // The bullet then homes in from any distance, no detection zone needed.
        public void SetTarget(Transform target)
        {
            _target = target;
        }

        // Called by Unity when you add the component in the editor.
        // GetComponentInParent works whether this sits on the bullet root or on a child collider.
        private void Reset()
        {
            bulletVar = GetComponentInParent<BulletVar>();
        }

        private void Awake()
        {
            if (TryGetComponent(out _zone) && !_zone.IsTrigger)
            {
                _zone = null;
            }
        }

        // Detection-zone mode: lock onto the first character inside the zone
        private void FindTarget()
        {
            FlatWorld.Overlap(_zone, InZone);

            foreach (FlatCollider other in InZone)
            {
                // Never home in on whoever fired the bullet (including their child colliders)
                if (bulletVar.bulletOwner != null && other.transform.IsChildOf(bulletVar.bulletOwner.transform))
                {
                    continue;
                }

                // Only lock onto characters, not walls
                if (other.TryGetComponent<PlayerStats>(out _))
                {
                    _target = other.transform;
                    return;
                }
            }
        }

        private void Update()
        {
            if (_target == null && _zone != null)
            {
                FindTarget();
            }

            // Unity's == null also covers the target having been destroyed
            if (_target == null)
            {
                return;
            }

            // Flat direction to the target, height ignored
            Vector3 toTarget = GamePlane.Direction(transform.position, _target.position);

            if (toTarget == Vector3.zero)
            {
                return;
            }

            float maxTurn = turnSpeed * Mathf.Deg2Rad * Time.deltaTime;
            Vector3 newDirection = Vector3.RotateTowards(bulletVar.bulletDirection, toTarget, maxTurn, 0f);

            bulletVar.bulletDirection = newDirection;
            transform.rotation = Quaternion.LookRotation(newDirection);
        }
    }
}