using System.Collections.Generic;
using _Project.Scripts.FlatPhysics;
using _Project.Scripts.Player;
using _Project.Scripts.Projectiles;
using _Project.Scripts.Utilities;
using UnityEngine;

// Put this on the tower's range: a FlatCollider set to Is Trigger. Fires at the closest character inside it on a cooldown.
[RequireComponent(typeof(FlatCollider))]
public class TowerShoot : MonoBehaviour
{
    [SerializeField] private BulletVar bulletPrefab;

    [Tooltip("Where bullets spawn. Falls back to this object's position if empty.")]
    [SerializeField] private Transform firePoint;

    [Tooltip("The tower that owns the bullets (they won't hit it). Falls back to the parent object.")]
    [SerializeField] private GameObject owner;

    [Tooltip("Seconds between shots")]
    [SerializeField, Min(0.05f)] private float fireInterval = 1.5f;

    // Shared by every tower: only used inside one call
    private static readonly List<FlatCollider> InRange = new List<FlatCollider>();

    private FlatCollider _range;
    private float _nextFireTime;

    private void Awake()
    {
        _range = GetComponent<FlatCollider>();

        if (owner == null)
        {
            owner = transform.parent != null ? transform.parent.gameObject : gameObject;
        }

        if (firePoint == null)
        {
            firePoint = transform;
        }
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
        FlatWorld.Overlap(_range, InRange);

        Transform closest = null;
        float closestSqrDistance = float.MaxValue;
        Vector2 center = _range.Center;

        foreach (FlatCollider candidate in InRange)
        {
            // Never the tower itself, and only characters count as targets
            if (candidate.transform.IsChildOf(owner.transform) || !candidate.TryGetComponent<PlayerStats>(out _))
            {
                continue;
            }

            float sqrDistance = (candidate.Center - center).sqrMagnitude;
            if (sqrDistance < closestSqrDistance)
            {
                closestSqrDistance = sqrDistance;
                closest = candidate.transform;
            }
        }

        return closest;
    }

    private bool TryFireAt(Transform target)
    {
        // Flat direction like every other bullet, height ignored
        Vector3 direction = GamePlane.Direction(firePoint.position, target.position);

        if (direction == Vector3.zero)
        {
            return false;
        }

        BulletVar bullet = Instantiate(bulletPrefab, GamePlane.AtProjectileHeight(firePoint.position), Quaternion.LookRotation(direction));
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