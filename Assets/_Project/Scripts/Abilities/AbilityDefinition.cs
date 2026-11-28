using UnityEngine;

// Base class for every ability. In most cases don't inherit from this directly,
// pick the base class that matches how the ability is used:
//   InstantAbility - does its thing when you press the button (basic attacks, walls, dashes)
//   TwoStepAbility - press once to ready, press again to launch (stone lob)
//   ToggleAbility  - press to switch on, press again to switch off (mounts, shields)
namespace _Project.Scripts.Abilities
{
    public abstract class AbilityDefinition : ScriptableObject
    {
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private Sprite icon;

        [Tooltip("Seconds before this ability can be used again")]
        [SerializeField, Min(0f)] private float cooldown = 1f;

        [Tooltip("Paid with whatever the character uses (mana, energy, HP...). 0 = free. Ignored by characters without a resource")]
        [SerializeField, Min(0f)] private float cost;

        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public float Cooldown => cooldown;
        public float Cost => cost;

        // True while the ability is in the middle of something (readied, switched on...). For UI and queries.
        public virtual bool IsActive => false;

        // The caster works on its own private copy of this asset, so fields you add in
        // subclasses are safe to use as per-caster state (timers, "is toggled on", etc.)

        // Called when the ability is put into a slot (good for passives)
        public virtual void OnEquipped(AbilityCaster caster) { }

        // Called when it is removed (undo whatever OnEquipped set up)
        public virtual void OnUnequipped(AbilityCaster caster) { }

        // Called every frame while the ability is equipped (timers, toggles, previews like a range circle)
        public virtual void OnTick(AbilityCaster caster) { }

        // Interrupt whatever the ability is doing (stun, death, character swap). Safe to call at any time.
        public virtual void Cancel(AbilityCaster caster) { }

        // Called when the button is pressed. Return true to start the cooldown.
        public abstract bool Activate(in AbilityContext context);
    }
}