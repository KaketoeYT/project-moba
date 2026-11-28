using System;
using _Project.Scripts.Abilities;
using UnityEngine;

namespace _Project.Scripts.Controls
{
    public class PlayerInputs : MonoBehaviour
    {
        [Serializable]
        private struct SlotBinding
        {
            public AbilitySlot slot;
            public KeyCode key;
        }

        [SerializeField] private AbilityCaster caster;

        // Falls back to the one on the main camera if left empty
        [SerializeField] private WorldMousePos mousePos;

        [Tooltip("Button name from Edit > Project Settings > Input Manager")]
        [SerializeField] private string basicAttackButton = "Fire1";

        // Rebindable in the Inspector. The Passive slot has no key, it just works.
        [SerializeField] private SlotBinding[] abilityBindings =
        {
            new SlotBinding { slot = AbilitySlot.Ability1, key = KeyCode.Q },
            new SlotBinding { slot = AbilitySlot.Ability2, key = KeyCode.E },
            new SlotBinding { slot = AbilitySlot.Ability3, key = KeyCode.LeftShift },
            new SlotBinding { slot = AbilitySlot.Ultimate, key = KeyCode.R },
        };

        // Called by Unity when you add the component in the editor: auto-fills the reference
        private void Reset()
        {
            caster = GetComponent<AbilityCaster>();
        }

        // The key bound to a slot, so UI can show it. False if the slot has no key.
        public bool TryGetKey(AbilitySlot slot, out KeyCode key)
        {
            foreach (SlotBinding binding in abilityBindings)
            {
                if (binding.slot == slot)
                {
                    key = binding.key;
                    return true;
                }
            }

            key = KeyCode.None;
            return false;
        }

        private void Awake()
        {
            if (mousePos == null && !Camera.main.TryGetComponent(out mousePos))
            {
                Debug.LogError($"{nameof(PlayerInputs)} on {name}: no WorldMousePos found on the main camera.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            Vector3 aimPoint = mousePos.Hit;

            if (UnityEngine.Input.GetButtonDown(basicAttackButton))
            {
                caster.TryCast(AbilitySlot.BasicAttack, aimPoint);
            }

            foreach (SlotBinding binding in abilityBindings)
            {
                if (UnityEngine.Input.GetKeyDown(binding.key))
                {
                    caster.TryCast(binding.slot, aimPoint);
                }
            }
        }
    }
}