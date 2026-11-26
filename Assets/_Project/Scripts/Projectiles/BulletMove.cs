using UnityEngine;

namespace _Project.Scripts.Projectiles
{
    [RequireComponent(typeof(BulletVar))]
    public class BulletMove : MonoBehaviour
    {
        private void Reset()
        {
            bulletVar = GetComponent<BulletVar>();
        }
    
        [SerializeField] private BulletVar bulletVar;
    
        private void Start()
        {
            // Safety net: despawn after the configured lifetime
            Destroy(gameObject, bulletVar.bulletDeathTime);
        }

        private void Update()
        {
            // bulletDirection is already a normalized direction (set by whoever spawned the bullet)
            transform.position += bulletVar.bulletDirection * (bulletVar.bulletSpeed * Time.deltaTime);
        }
    }
}