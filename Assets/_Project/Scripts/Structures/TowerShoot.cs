using System.Collections.Generic;
using _Project.Scripts.Player;
using _Project.Scripts.Projectiles;
using UnityEngine;

// Put this on the tower's range trigger. Fires at the closest valid target in range on a cooldown.
[RequireComponent(typeof(Collider))]
public class TowerShoot : MonoBehaviour
{
    [SerializeField] private BulletVar bulletPrefab;

    [Tooltip("Where bullets spawn. Falls back to this object's position if empty.")]
    [SerializeField] private Transform firePoint;

    [Tooltip("The tower that owns the bullets (they won't hit it). Falls back to the parent object.")]
    [SerializeField] private GameObject owner;

    [Tooltip("Seconds between shots")]
    [SerializeField, Min(0.05f)] private float fireInterval = 1.5f;

    private readonly List<Transform> _targets = new List<Transform>();
    private float _nextFireTime;

    private void Awake()
    {
        if (owner == null)
        {
            owner = transform.parent != null ? transform.parent.gameObject : gameObject;
        }

        if (firePoint == null)
        {
            firePoint = transform;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignore trigger zones and the tower's own colliders
        if (other.isTrigger || other.transform.IsChildOf(owner.transform))
        {
            return;
        }

        // Only things that can take damage count as targets
        if (!other.TryGetComponent<PlayerStats>(out _))
        {
            return;
        }

        if (!_targets.Contains(other.transform))
        {
            _targets.Add(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _targets.Remove(other.transform);
    }

    private void Update()
    {
        if (Time.time < _nextFireTime)
        {
            return;
        }

        Transform target = GetClosestTarget();
        if (target == null)
        {
            return;
        }

        if (TryFireAt(target))
        {
            _nextFireTime = Time.time + fireInterval;
        }
    }

    private Transform GetClosestTarget()
    {
        Transform closest = null;
        float closestSqrDistance = float.MaxValue;

        // Loop backwards so destroyed targets can be removed safely while iterating.
        // OnTriggerExit isn't called for objects that get destroyed inside the range.
        for (int i = _targets.Count - 1; i >= 0; i--)
        {
            Transform candidate = _targets[i];

            if (candidate == null)
            {
                _targets.RemoveAt(i);
                continue;
            }

            float sqrDistance = (candidate.position - transform.position).sqrMagnitude;
            if (sqrDistance < closestSqrDistance)
            {
                closestSqrDistance = sqrDistance;
                closest = candidate;
            }
        }

        return closest;
    }

    private bool TryFireAt(Transform target)
    {
        // Flat (XZ) direction for the first moment of flight, same convention as every other bullet
        Vector3 direction = target.position - firePoint.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
        {
            return false;
        }

        direction.Normalize();

        BulletVar bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(direction));
        bullet.bulletOwner = owner;
        bullet.bulletDirection = direction;

        // If the bullet can home, lock it onto its target so it follows from any distance
        if (bullet.TryGetComponent(out BulletHoming homing))
        {
            homing.SetTarget(target);
        }

        return true;
    }
}