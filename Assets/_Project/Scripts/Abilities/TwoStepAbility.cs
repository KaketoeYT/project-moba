using UnityEngine;
using UnityEngine.Serialization;

// Press once to ready, press again to launch. The cost is paid and the cooldown starts when it launches
// (you need enough to ready it too, so you don't ready something you can't throw).
// Inherit from this and write Launch; override the On... methods only if you need them.
namespace _Project.Scripts.Abilities
{
    public abstract class TwoStepAbility : AbilityDefinition
    {
        [Header("Step 1: Ready")]
        [Tooltip("Seconds the caster can't move while readying (first press)")]
        [SerializeField, Min(0f)] private float readyStandStill;

        [Tooltip("Seconds until a readied ability is dropped on its own (0 = waits forever)")]
        [SerializeField, Min(0f)] private float readyTimeout;
        
        [Tooltip("Cooldown that starts if the readied ability runs out unused (0 = no cooldown)")]
        [SerializeField, Min(0f)] private float timeoutCooldown;

        [Header("Step 2: Launch")]
        [Tooltip("Seconds the caster can't move after launching (second press)")]
        [FormerlySerializedAs("standStillDuration")]
        [SerializeField, Min(0f)] private float launchStandStill;

        // Per-caster state. Safe, because every caster works on its own private copy of this asset.
        private bool _isReady;
        private float _dropAt;

        public sealed override bool IsActive => _isReady;

        public sealed override bool Activate(in AbilityContext context)
        {
            if (!context.Caster.CanAfford(Cost))
            {
                return false;
            }

            // Step 1: ready. No cooldown yet, so we return false.
            if (!_isReady)
            {
                _isReady = true;
                _dropAt = readyTimeout > 0f ? Time.time + readyTimeout : float.PositiveInfinity;
                OnReady(in context);
                context.Caster.LockMovement(readyStandStill);
                return false;
            }

            // Step 2: launch. If it can't launch (e.g. missing prefab) it stays readied.
            if (!Launch(in context))
            {
                return false;
            }

            _isReady = false;
            context.Caster.TryPay(Cost);
            context.Caster.LockMovement(launchStandStill);
            return true;
        }

        public sealed override void OnTick(AbilityCaster caster)
        {
            if (!_isReady)
            {
                return;
            }

            if (Time.time >= _dropAt)
            {
                Cancel(caster);
                caster.StartCooldown(this, timeoutCooldown);
                return;
            }

            OnReadyTick(caster);
        }

        // Drops a readied ability without launching it. No cooldown, since nothing happened.
        public sealed override void Cancel(AbilityCaster caster)
        {
            if (!_isReady)
            {
                return;
            }

            _isReady = false;
            OnCancelled(caster);
        }

        public sealed override void OnUnequipped(AbilityCaster caster)
        {
            Cancel(caster);
        }

        // Called once when the first press readies the ability
        protected virtual void OnReady(in AbilityContext context) { }

        // Called every frame while readied (range previews, indicators...)
        protected virtual void OnReadyTick(AbilityCaster caster) { }

        // Called when a readied ability is dropped (timeout, interrupt, unequip)
        protected virtual void OnCancelled(AbilityCaster caster) { }

        // The second press. Return false if it couldn't launch: it then stays readied.
        protected abstract bool Launch(in AbilityContext context);
    }
}