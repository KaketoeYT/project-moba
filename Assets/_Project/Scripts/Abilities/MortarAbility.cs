using _Project.Scripts.Projectiles;
using _Project.Scripts.Utilities;
using UnityEngine;

// Ready a projectile, then launch it in an arc to the aim point.
namespace _Project.Scripts.Abilities
{
    [CreateAssetMenu(menuName = "MOBA/Abilities/Mortar (two-step)", fileName = "NewMortarAbility")]
    public class MortarAbility : TwoStepAbility
    {
        [SerializeField] private ArcProjectile projectilePrefab;

        [Tooltip("Furthest distance the projectile can land, in units")]
        [SerializeField, Min(1f)] private float maxRange = 25f;

        // Range preview while readied
        protected override void OnReadyTick(AbilityCaster caster)
        {
            DebugDraw.CircleOnGround(caster.CastOrigin.position, maxRange, Color.yellow);
        }

        protected override bool Launch(in AbilityContext context)
        {
            if (projectilePrefab == null)
            {
                Debug.LogError($"{name}: no projectile prefab assigned.", this);
                return false;
            }

            // Aiming further than maxRange lands the projectile at maxRange instead.
            // Everything is worked out on the flat ground plane, so heights never matter.
            Vector3 origin = GamePlane.OnGround(context.Origin);
            Vector3 target = GamePlane.OnGround(context.AimPoint);
            Vector3 offset = target - origin;

            if (offset.magnitude > maxRange)
            {
                target = origin + offset.normalized * maxRange;
            }

            ArcProjectile projectile = Instantiate(projectilePrefab, GamePlane.AtProjectileHeight(context.Origin), Quaternion.identity);
            projectile.Launch(context.Owner, target);
            return true;
        }
    }
}