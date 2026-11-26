using UnityEngine;

// Put this on anything that can use abilities (players now, maybe towers or minions later).
// It knows nothing about input: whoever controls the character calls TryCast.
namespace _Project.Scripts.Abilities
{
    public class AbilityCaster : MonoBehaviour
    {
        [Tooltip("Where abilities start from (e.g. the ballista on a character's back). Falls back to this object.")]
        [SerializeField] private Transform castOrigin;

        private sealed class SlotState
        {
            public AbilityDefinition Source;  // the asset you assigned
            public AbilityDefinition Runtime; // this caster's own private copy
            public float ReadyTime;           // Time.time when it can be cast again
        }

        // Created here (not in Awake) so Equip works no matter which script runs first
        private readonly SlotState[] _slots = CreateSlots();

        public Transform CastOrigin => castOrigin != null ? castOrigin : transform;

        private static SlotState[] CreateSlots()
        {
            var slots = new SlotState[AbilitySlots.All.Length];
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i] = new SlotState();
            }

            return slots;
        }

        // Put an ability in a slot (null empties it). Replaces whatever was there.
        // This is the one method that makes mix-and-match and ability stealing possible.
        public void Equip(AbilitySlot slot, AbilityDefinition definition)
        {
            Unequip(slot);

            if (definition == null)
            {
                return;
            }

            SlotState state = _slots[(int)slot];
            state.Source = definition;
            state.Runtime = Instantiate(definition);
            state.Runtime.name = definition.name;
            state.ReadyTime = 0f;
            state.Runtime.OnEquipped(this);
        }

        public void Unequip(AbilitySlot slot)
        {
            SlotState state = _slots[(int)slot];

            if (state.Runtime == null)
            {
                return;
            }

            state.Runtime.OnUnequipped(this);
            Destroy(state.Runtime);
            state.Source = null;
            state.Runtime = null;
        }

        // Returns true if the ability fired
        public bool TryCast(AbilitySlot slot, Vector3 aimPoint)
        {
            SlotState state = _slots[(int)slot];

            if (state.Runtime == null || Time.time < state.ReadyTime)
            {
                return false;
            }

            var context = new AbilityContext(this, aimPoint);

            if (!state.Runtime.Activate(in context))
            {
                return false;
            }

            state.ReadyTime = Time.time + state.Runtime.Cooldown;
            return true;
        }

        // The asset in a slot (null if empty). Handy for copying another caster's ability.
        public AbilityDefinition GetDefinition(AbilitySlot slot)
        {
            return _slots[(int)slot].Source;
        }

        // Seconds left on the cooldown (0 = ready). For UI later.
        public float GetCooldownRemaining(AbilitySlot slot)
        {
            return Mathf.Max(0f, _slots[(int)slot].ReadyTime - Time.time);
        }

        private void Update()
        {
            // Let every equipped ability do its per-frame work (timers, range previews...)
            foreach (SlotState state in _slots)
            {
                if (state.Runtime != null)
                {
                    state.Runtime.OnTick(this);
                }
            }
        }
        
        private void OnDestroy()
        {
            foreach (AbilitySlot slot in AbilitySlots.All)
            {
                Unequip(slot);
            }
        }
    }
}