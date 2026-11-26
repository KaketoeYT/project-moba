using UnityEngine;

namespace _Project.Scripts.Projectiles
{
    public class BulletVar : MonoBehaviour
    {
        // Set by code when the bullet is spawned, so there's no reason to show them in the Inspector
        [HideInInspector] public GameObject bulletOwner;
        [HideInInspector] public Vector3 bulletDirection;

        [Tooltip("Seconds before the bullet despawns on its own")]
        [Min(0f)] public float bulletDeathTime = 2f;

        [Tooltip("Units per second")]
        [Min(0f)] public float bulletSpeed = 10f;

        [Min(0f)] public float bulletDamage = 10f;
    }
}