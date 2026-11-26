using _Project.Scripts.Player;
using UnityEngine;

namespace _Project.Scripts.Projectiles
{
    public class BulletHoming : MonoBehaviour
    {
        [SerializeField] private BulletVar bulletVar;

        [Tooltip("How fast the bullet can turn towards its target, in degrees per second")]
        [SerializeField, Min(0f)] private float turnSpeed = 360f;

        [Tooltip("On: only steer sideways (bullet keeps its spawn height). Off: also steer up and down.")]
        [SerializeField] private bool steerOnFlatPlane = true;

        private Transform _target;

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

        // Detection-zone mode: when this sits on a trigger collider, lock onto the first valid thing that enters
        private void OnTriggerEnter(Collider other)
        {
            // Already locked on to something
            if (_target != null)
            {
                return;
            }

            // Ignore other trigger zones
            if (other.isTrigger)
            {
                return;
            }

            // Never home in on whoever fired the bullet (including their child colliders)
            if (bulletVar.bulletOwner != null && other.transform.IsChildOf(bulletVar.bulletOwner.transform))
            {
                return;
            }

            // Only lock onto things that can actually take damage, not walls or terrain
            if (!other.TryGetComponent<PlayerStats>(out _))
            {
                return;
            }

            _target = other.transform;
        }

        private void Update()
        {
            // Unity's == null also covers the target having been destroyed
            if (_target == null)
            {
                return;
            }

            Vector3 toTarget = _target.position - transform.position;

            if (steerOnFlatPlane)
            {
                toTarget.y = 0f;
            }

            if (toTarget.sqrMagnitude < 0.0001f)
            {
                return;
            }

            float maxTurn = turnSpeed * Mathf.Deg2Rad * Time.deltaTime;
            Vector3 newDirection = Vector3.RotateTowards(bulletVar.bulletDirection, toTarget.normalized, maxTurn, 0f);

            bulletVar.bulletDirection = newDirection;
            transform.rotation = Quaternion.LookRotation(newDirection);
        }
    }
}