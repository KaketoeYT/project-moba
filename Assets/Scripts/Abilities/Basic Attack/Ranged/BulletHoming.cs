using UnityEngine;

public class BulletHoming : MonoBehaviour
{
    [SerializeField] private BulletVar bulletVar;

    [Tooltip("How fast the bullet can turn towards its target, in degrees per second")]
    [SerializeField, Min(0f)] private float turnSpeed = 360f;

    private Transform _target;

    // This script lives on a trigger collider that acts as the bullet's "detection radius"
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

        // Steer on the flat XZ plane only, same as the rest of the bullet logic
        Vector3 toTarget = _target.position - transform.position;
        toTarget.y = 0f;

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