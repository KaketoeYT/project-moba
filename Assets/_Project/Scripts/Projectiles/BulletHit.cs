using UnityEngine;

public class BulletHit : MonoBehaviour
{
    [SerializeField] private BulletVar bulletVar;

    // Prevents hitting two colliders in the same frame and dealing damage twice
    private bool _hasHit;

    private void OnTriggerEnter(Collider other)
    {
        if (_hasHit)
        {
            return;
        }

        // Ignore trigger zones (tower ranges, homing zones, other bullets)
        if (other.isTrigger)
        {
            return;
        }

        // Ignore whoever fired this bullet, including their child colliders
        if (bulletVar.bulletOwner != null && other.transform.IsChildOf(bulletVar.bulletOwner.transform))
        {
            return;
        }

        _hasHit = true;

        // Only things with PlayerStats take damage; anything else solid (walls, terrain) just stops the bullet
        if (other.TryGetComponent(out PlayerStats stats))
        {
            stats.TakeDamage(bulletVar.bulletDamage);
        }

        Destroy(gameObject);
    }
}