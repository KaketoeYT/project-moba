using UnityEngine;

// An ability that does its thing the moment you press the button (basic attacks, walls, dashes...).
// Inherit from this and only write Perform.
namespace _Project.Scripts.Abilities
{
    public abstract class InstantAbility : AbilityDefinition
    {
        [Tooltip("Seconds the caster can't move after using this ability (0 = can keep moving)")]
        [SerializeField, Min(0f)] private float standStillDuration;

        public sealed override bool Activate(in AbilityContext context)
        {
            if (!Perform(in context))
            {
                return false;
            }

            context.Caster.LockMovement(standStillDuration);
            return true;
        }

        // Do the ability. Return false if nothing happened (no cooldown, no stand-still).
        protected abstract bool Perform(in AbilityContext context);
    }
}