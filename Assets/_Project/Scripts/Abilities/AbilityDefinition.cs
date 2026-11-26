using UnityEngine;

// Base class for every ability. Make a new kind of ability by inheriting from this.
namespace _Project.Scripts.Abilities
{
    public abstract class AbilityDefinition : ScriptableObject
    {
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private Sprite icon;

        [Tooltip("Seconds before this ability can be used again")]
        [SerializeField, Min(0f)] private float cooldown = 1f;

        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public float Cooldown => cooldown;

        // The caster works on its own private copy of this asset, so fields you add in
        // subclasses are safe to use as per-caster state (timers, "is toggled on", etc.)

        // Called when the ability is put into a slot (good for passives)
        public virtual void OnEquipped(AbilityCaster caster) { }

        // Called when it is removed (undo whatever OnEquipped set up)
        public virtual void OnUnequipped(AbilityCaster caster) { }
        
        // Called every frame while the ability is equipped (timers, toggles, previews like a range circle)
        public virtual void OnTick(AbilityCaster caster) { }

        // Return true if the ability really fired: the cooldown only starts then.
        // Return false for "nothing happened" or for the first step of a two-step cast.
        public abstract bool Activate(in AbilityContext context);
    }
}