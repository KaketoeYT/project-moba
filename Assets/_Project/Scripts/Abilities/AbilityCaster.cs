using _Project.Scripts.Player;
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

        // Optional: lets abilities make the caster stand still. Found automatically if on the same object.
        private MovementLock _movementLock;

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

        private void Awake()
        {
            TryGetComponent(out _movementLock);
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

        // Returns true if the ability fired and started its cooldown
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

        // Starts the full cooldown of an equipped ability. For abilities that end on their own
        // (a toggle that times out), so they can't rely on returning true from Activate.
        public void StartCooldown(AbilityDefinition ability)
        {
            StartCooldown(ability, ability.Cooldown);
        }

        // Starts a cooldown of a custom length (e.g. the shorter one after a readied ability times out)
        public void StartCooldown(AbilityDefinition ability, float duration)
        {
            if (duration <= 0f)
            {
                return;
            }

            foreach (SlotState state in _slots)
            {
                if (state.Runtime == ability)
                {
                    state.ReadyTime = Time.time + duration;
                    return;
                }
            }
        }

        // Stops this caster from moving for a while. Abilities call this for their stand-still.
        public void LockMovement(float duration)
        {
            if (_movementLock != null && duration > 0f)
            {
                _movementLock.Lock(duration);
            }
        }

        // Interrupts everything the caster is doing (stun, death...)
        public void CancelAll()
        {
            foreach (SlotState state in _slots)
            {
                if (state.Runtime != null)
                {
                    state.Runtime.Cancel(this);
                }
            }
        }

        // The asset in a slot (null if empty). Handy for copying another caster's ability.
        public AbilityDefinition GetDefinition(AbilitySlot slot)
        {
            return _slots[(int)slot].Source;
        }

        // True while the ability in that slot is readied or switched on. For UI later.
        public bool IsActive(AbilitySlot slot)
        {
            AbilityDefinition runtime = _slots[(int)slot].Runtime;
            return runtime != null && runtime.IsActive;
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