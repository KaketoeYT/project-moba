using _Project.Scripts.Structures;
using _Project.Scripts.Utilities;
using UnityEngine;

// Places a wall at the aim point (within range), turned to face the caster.
namespace _Project.Scripts.Abilities
{
    [CreateAssetMenu(menuName = "MOBA/Abilities/Place Wall", fileName = "NewWallAbility")]
    public class WallAbility : InstantAbility
    {
        [SerializeField] private Wall wallPrefab;

        [Tooltip("Furthest distance from the caster the wall can be placed")]
        [SerializeField, Min(1f)] private float maxRange = 8f;

        [Tooltip("Closest the wall can be placed, so it never appears on top of the caster")]
        [SerializeField, Min(0f)] private float minRange = 2f;

        protected override bool Perform(in AbilityContext context)
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

            Vector3 origin = GamePlane.OnGround(context.Origin);
            Vector3 aim = GamePlane.OnGround(context.AimPoint);
            float distance = Mathf.Clamp(Vector3.Distance(origin, aim), minRange, maxRange);

            // Everything is placed on the ground plane, whatever is under the cursor
            Vector3 position = origin + context.AimDirection * distance;

            // The wall's flat face points back at the caster
            Quaternion rotation = Quaternion.LookRotation(context.AimDirection);

            Instantiate(wallPrefab, position, rotation);
            return true;
        }
    }
}