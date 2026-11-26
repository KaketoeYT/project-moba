using _Project.Scripts.Projectiles;
using UnityEngine;

// Two-step ability: the first cast readies a projectile, the second launches it in an arc to the aim point.
namespace _Project.Scripts.Abilities
{
    [CreateAssetMenu(menuName = "MOBA/Abilities/Mortar (two-step)", fileName = "NewMortarAbility")]
    public class MortarAbility : AbilityDefinition
    {
        [SerializeField] private ArcProjectile projectilePrefab;

        [Tooltip("Furthest distance the projectile can land, in units")]
        [SerializeField, Min(1f)] private float maxRange = 25f;

        // Per-caster state. Safe, because every caster works on its own private copy of this asset.
        private bool _isReady;

        public override void OnEquipped(AbilityCaster caster)
        {
            _isReady = false;
        }

        public override bool Activate(in AbilityContext context)
        {
            // Step 1: ready the projectile. Returning false means the cooldown does NOT start yet.
            if (!_isReady)
            {
                _isReady = true;
                return false;
            }

            if (projectilePrefab == null)
            {
                Debug.LogError($"{name}: no projectile prefab assigned.", this);
                return false;
            }

            // Step 2: launch. Aiming further than maxRange lands the projectile at maxRange instead.
            Vector3 target = context.AimPoint;
            Vector3 offset = target - context.Origin;
            offset.y = 0f;

            if (offset.magnitude > maxRange)
            {
                float groundHeight = target.y;
                target = context.Origin + offset.normalized * maxRange;
                target.y = groundHeight;
            }

            ArcProjectile projectile = Instantiate(projectilePrefab, context.Origin, Quaternion.identity);
            projectile.Launch(context.Owner, target);

            _isReady = false;
            return true;
        }
    }
}