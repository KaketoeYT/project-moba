using UnityEngine;

// Press to switch on, press again to switch off. The cooldown starts when it switches OFF,
// including when it switches itself off (max duration, interrupt, unequip).
// Inherit from this and override the On... methods you need.
namespace _Project.Scripts.Abilities
{
    public abstract class ToggleAbility : AbilityDefinition
    {
        [Header("Switching on")]
        [Tooltip("Seconds the caster can't move when switching on")]
        [SerializeField, Min(0f)] private float switchOnStandStill;

        [Header("Switching off")]
        [Tooltip("Seconds the caster can't move when switching off with the button")]
        [SerializeField, Min(0f)] private float switchOffStandStill;

        [Tooltip("Seconds until it switches itself off (0 = stays on until you switch it off)")]
        [SerializeField, Min(0f)] private float maxDuration;

        // Per-caster state. Safe, because every caster works on its own private copy of this asset.
        private bool _isOn;
        private float _offAt;

        public sealed override bool IsActive => _isOn;

        public sealed override bool Activate(in AbilityContext context)
        {
            if (!_isOn)
            {
                if (!CanSwitchOn(in context))
                {
                    return false;
                }

                _isOn = true;
                _offAt = maxDuration > 0f ? Time.time + maxDuration : float.PositiveInfinity;
                OnSwitchedOn(in context);
                context.Caster.LockMovement(switchOnStandStill);
            }
            else
            {
                SwitchOff(context.Caster);
                context.Caster.LockMovement(switchOffStandStill);
            }

            // Always false: SwitchOff starts the cooldown itself, so it also works when the toggle ends on its own
            return false;
        }

        public sealed override void OnTick(AbilityCaster caster)
        {
            if (!_isOn)
            {
                return;
            }

            if (Time.time >= _offAt)
            {
                SwitchOff(caster);
                return;
            }

            OnActiveTick(caster);
        }

        public sealed override void Cancel(AbilityCaster caster)
        {
            if (_isOn)
            {
                SwitchOff(caster);
            }
        }

        public sealed override void OnUnequipped(AbilityCaster caster)
        {
            Cancel(caster);
        }

        private void SwitchOff(AbilityCaster caster)
        {
            _isOn = false;
            OnSwitchedOff(caster);
            caster.StartCooldown(this);
        }

        // Return false to refuse switching on (e.g. not enough room to mount)
        protected virtual bool CanSwitchOn(in AbilityContext context) => true;

        protected virtual void OnSwitchedOn(in AbilityContext context) { }

        // Called every frame while switched on
        protected virtual void OnActiveTick(AbilityCaster caster) { }

        // Undo whatever OnSwitchedOn set up. Called however the toggle ends.
        protected virtual void OnSwitchedOff(AbilityCaster caster) { }
    }
}