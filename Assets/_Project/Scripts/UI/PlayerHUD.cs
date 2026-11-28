using System;
using _Project.Scripts.Abilities;
using _Project.Scripts.Controls;
using _Project.Scripts.Player;
using UnityEngine;

// The player's on-screen info: ability squares, a health bar and a resource bar.
// Point it at the player, or leave it empty to use the object tagged "Player" / the one with PlayerInputs.
// Build a ready-made one with the menu: MOBA > Create Player HUD.
namespace _Project.Scripts.UI
{
    public class PlayerHUD : MonoBehaviour
    {
        [Serializable]
        private struct SlotView
        {
            public AbilitySlot slot;
            public AbilitySlotUI view;
        }

        [Tooltip("The player to show. Leave empty to use the object tagged \"Player\", or else the one with PlayerInputs.")]
        [SerializeField] private GameObject player;

        [SerializeField] private HealthBarUI healthBar;

        [SerializeField] private ResourceBarUI resourceBar;

        [SerializeField] private SlotView[] slots =
        {
            new SlotView { slot = AbilitySlot.Ability1 },
            new SlotView { slot = AbilitySlot.Ability2 },
            new SlotView { slot = AbilitySlot.Ability3 },
            new SlotView { slot = AbilitySlot.Ultimate },
        };

        private void Start()
        {
            if (player == null)
            {
                player = GameObject.FindWithTag("Player");
            }

            // Otherwise: whoever is controlled by the keyboard
            if (player == null)
            {
                var inputs = FindAnyObjectByType<PlayerInputs>();
                player = inputs != null ? inputs.gameObject : null;
            }

            if (player == null)
            {
                Debug.LogWarning($"{nameof(PlayerHUD)} on {name}: no player assigned, none tagged \"Player\" and no PlayerInputs in the scene.", this);
                return;
            }

            SetPlayer(player);
        }

        // Call this to show someone else (respawn, character swap, spectating...)
        public void SetPlayer(GameObject newPlayer)
        {
            player = newPlayer;

            newPlayer.TryGetComponent(out AbilityCaster caster);
            newPlayer.TryGetComponent(out PlayerStats stats);
            newPlayer.TryGetComponent(out PlayerInputs inputs);
            newPlayer.TryGetComponent(out CharacterResource resource);

            if (healthBar != null)
            {
                healthBar.SetTarget(stats);
            }

            if (resourceBar != null)
            {
                resourceBar.SetTarget(resource);
            }

            foreach (SlotView slotView in slots)
            {
                if (slotView.view == null)
                {
                    continue;
                }

                string keyLabel = inputs != null && inputs.TryGetKey(slotView.slot, out KeyCode key)
                    ? KeyLabel(key)
                    : string.Empty;

                slotView.view.Bind(caster, slotView.slot, keyLabel);
            }
        }

        // Short names for keys that would not fit in a small square
        private static string KeyLabel(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.LeftShift:
                case KeyCode.RightShift: return "Shift";
                case KeyCode.LeftControl:
                case KeyCode.RightControl: return "Ctrl";
                case KeyCode.LeftAlt:
                case KeyCode.RightAlt: return "Alt";
                case KeyCode.Space: return "Space";
                case KeyCode.Mouse0: return "LMB";
                case KeyCode.Mouse1: return "RMB";
                default:
                    if (key >= KeyCode.Alpha0 && key <= KeyCode.Alpha9)
                    {
                        return ((int)(key - KeyCode.Alpha0)).ToString();
                    }

                    return key.ToString();
            }
        }
    }
}
