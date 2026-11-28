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
            public float CooldownLength;      // length of the cooldown that is running (for UI)
        }

        // Created here (not in Awake) so Equip works no matter which script runs first
        private readonly SlotState[] _slots = CreateSlots();

        // Optional: lets abilities make the caster stand still. Found automatically if on the same object.
        private MovementLock _movementLock;

        // Optional: what abilities cost. Without one, everything is free.
        private CharacterResource _resource;

        public Transform CastOrigin => castOrigin != null ? castOrigin : transform;

        // What this caster pays with (null = everything is free). For UI.
        public CharacterResource Resource => FindResource();

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
            state.CooldownLength = 0f;
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
            state.CooldownLength = state.Runtime.Cooldown;
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
                    state.CooldownLength = duration;
                    return;
                }
            }
        }

        // True if the caster can pay this amount right now. Abilities check this before doing anything.
        public bool CanAfford(float cost)
        {
            return FindResource() == null || _resource.CanAfford(cost);
        }

        // True if the ability in this slot could be paid for right now. For UI (greying out icons).
        public bool CanAfford(AbilitySlot slot)
        {
            AbilityDefinition runtime = _slots[(int)slot].Runtime;
            return runtime == null || CanAfford(runtime.Cost);
        }

        // Pays a cost. Returns false (and pays nothing) if there isn't enough.
        // The ability base classes call this at the moment the ability really happens.
        public bool TryPay(float cost)
        {
            return FindResource() == null || _resource.TrySpend(cost);
        }

        // Looked up on demand, because CharacterLoadout may add the component after our Awake
        private CharacterResource FindResource()
        {
            if (_resource == null)
            {
                TryGetComponent(out _resource);
            }

            return _resource;
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

        // True while the ability in that slot is readied or switched on
        public bool IsActive(AbilitySlot slot)
        {
            AbilityDefinition runtime = _slots[(int)slot].Runtime;
            return runtime != null && runtime.IsActive;
        }

        // Seconds left on the cooldown (0 = ready)
        public float GetCooldownRemaining(AbilitySlot slot)
        {
            return Mathf.Max(0f, _slots[(int)slot].ReadyTime - Time.time);
        }

        // How much of the running cooldown is left, from 1 (just started) to 0 (ready). For cooldown overlays.
        public float GetCooldownFraction(AbilitySlot slot)
        {
            SlotState state = _slots[(int)slot];

            if (state.CooldownLength <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01((state.ReadyTime - Time.time) / state.CooldownLength);
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