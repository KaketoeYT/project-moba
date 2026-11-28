using _Project.Scripts.Projectiles;
using _Project.Scripts.Utilities;
using UnityEngine;

namespace _Project.Scripts.Abilities
{
    [CreateAssetMenu(menuName = "MOBA/Abilities/Projectile", fileName = "NewProjectileAbility")]
    public class ProjectileAbility : InstantAbility
    {
        [SerializeField] private BulletVar projectilePrefab;

        protected override bool Perform(in AbilityContext context)   
        {
            if (projectilePrefab == null)
            {
                Debug.LogError($"{name}: no projectile prefab assigned.", this);
                return false;
            }

            // Aiming at the caster's own feet: no meaningful direction, so don't fire
            if (context.AimDirection == Vector3.zero)
            {
                return false;
            }

            BulletVar bullet = Instantiate(projectilePrefab, GamePlane.AtProjectileHeight(context.Origin), Quaternion.LookRotation(context.AimDirection));
            bullet.bulletOwner = context.Owner;
            bullet.bulletDirection = context.AimDirection;
            return true;
        }
    }
}