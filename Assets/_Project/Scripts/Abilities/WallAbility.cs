using _Project.Scripts.Structures;
using UnityEngine;

// Places a wall at the aim point (within range), turned to face the caster.
namespace _Project.Scripts.Abilities
{
    [CreateAssetMenu(menuName = "MOBA/Abilities/Place Wall", fileName = "NewWallAbility")]
    public class WallAbility : AbilityDefinition
    {
        [SerializeField] private Wall wallPrefab;

        [Tooltip("Furthest distance from the caster the wall can be placed")]
        [SerializeField, Min(1f)] private float maxRange = 8f;

        [Tooltip("Closest the wall can be placed, so it never appears on top of the caster")]
        [SerializeField, Min(0f)] private float minRange = 2f;

        public override bool Activate(in AbilityContext context)
        {
            if (wallPrefab == null)
            {
                Debug.LogError($"{name}: no wall prefab assigned.", this);
                return false;
            }

            // Aiming at your own feet: no direction for the wall to face
            if (context.AimDirection == Vector3.zero)
            {
                return false;
            }

            Vector3 offset = context.AimPoint - context.Origin;
            offset.y = 0f;
            float distance = Mathf.Clamp(offset.magnitude, minRange, maxRange);

            Vector3 position = context.Origin + context.AimDirection * distance;
            position.y = FindGroundHeight(position, context.AimPoint.y);

            // The wall's flat face points back at the caster
            Quaternion rotation = Quaternion.LookRotation(context.AimDirection);

            Instantiate(wallPrefab, position, rotation);
            return true;
        }

        // The caster's height isn't the ground's height, so look straight down for the real ground
        private static float FindGroundHeight(Vector3 position, float fallback)
        {
            Vector3 rayStart = position + Vector3.up * 5f;

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 20f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                return hit.point.y;
            }

            return fallback;
        }
    }
}