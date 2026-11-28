using UnityEngine;

// The resource this character pays for abilities with. CharacterLoadout sets it up from the
// CharacterDefinition; the fields below are only for objects without a loadout.
namespace _Project.Scripts.Player
{
    public class CharacterResource : MonoBehaviour
    {
        [SerializeField] private ResourceMode mode;

        [Tooltip("Only used when Mode is Custom")]
        [SerializeField] private ResourceDefinition definition;

        [Tooltip("Only used when Mode is Custom: how much can be held")]
        [SerializeField, Min(1f)] private float max = 100f;

        [Tooltip("Only used when Mode is Custom: gained per second. Negative drains it on its own")]
        [SerializeField] private float regenPerSecond = 5f;

        private PlayerStats _stats;
        private float _current;

        public ResourceMode Mode => mode;
        public ResourceDefinition Definition => definition;

        // True when there is a separate bar to show (Health uses the HP bar, None has nothing)
        public bool HasOwnBar => mode == ResourceMode.Custom && definition != null;

        public float Current => HasOwnBar ? _current : 0f;
        public float Max => HasOwnBar ? max : 0f;
        public float RegenPerSecond => HasOwnBar ? regenPerSecond : 0f;

        private void Awake()
        {
            TryGetComponent(out _stats);
            Setup(mode, definition, max, regenPerSecond);
        }

        // Switch to another resource (character swap). Refills to the resource's start amount.
        // The amount and refill rate belong to the character, the definition only says what kind it is.
        public void Setup(ResourceMode newMode, ResourceDefinition newDefinition, float newMax, float newRegenPerSecond)
        {
            mode = newMode;
            definition = newDefinition;
            max = Mathf.Max(1f, newMax);
            regenPerSecond = newRegenPerSecond;
            _current = HasOwnBar ? max * definition.StartFraction : 0f;
        }

        // Change the cap or refill rate mid-game (levelling up, items, buffs). Keeps the current amount.
        public void SetStats(float newMax, float newRegenPerSecond)
        {
            max = Mathf.Max(1f, newMax);
            regenPerSecond = newRegenPerSecond;
            _current = Mathf.Min(_current, max);
        }

        public bool CanAfford(float cost)
        {
            if (cost <= 0f)
            {
                return true;
            }

            switch (mode)
            {
                // Must keep at least a sliver of HP: paying can't kill you
                case ResourceMode.Health: return _stats == null || _stats.Health > cost;
                case ResourceMode.Custom: return definition == null || _current >= cost;
                default: return true;
            }
        }

        // Pays the cost if possible. Returns false (and pays nothing) if there isn't enough.
        public bool TrySpend(float cost)
        {
            if (!CanAfford(cost))
            {
                return false;
            }

            if (cost <= 0f)
            {
                return true;
            }

            if (mode == ResourceMode.Health && _stats != null)
            {
                _stats.PayHealth(cost);
            }
            else if (HasOwnBar)
            {
                _current -= cost;
            }

            return true;
        }

        // Add some (hitting an enemy builds blood/rage, a mana potion...). Does nothing without a bar.
        public void Gain(float amount)
        {
            if (HasOwnBar)
            {
                _current = Mathf.Clamp(_current + amount, 0f, max);
            }
        }

        private void Update()
        {
            if (HasOwnBar && regenPerSecond != 0f)
            {
                Gain(regenPerSecond * Time.deltaTime);
            }
        }
    }
}
