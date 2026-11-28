using _Project.Scripts.Abilities;
using _Project.Scripts.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One square on the HUD: shows an ability's icon, its cost, its cooldown sweep and seconds left.
// PlayerHUD tells it which caster and slot to watch; it reads them every frame.
namespace _Project.Scripts.UI
{
    public class AbilitySlotUI : MonoBehaviour
    {
        [SerializeField] private Image icon;

        [Tooltip("Dark image drawn over the square. Set its Image Type to Filled (Radial 360) so it sweeps away. " +
                 "Leave its sprite empty to use a plain square that reaches the corners.")]
        [SerializeField] private Image cooldownOverlay;

        [SerializeField] private TMP_Text cooldownText;

        [Tooltip("Optional: the key that casts this slot (Q, E...)")]
        [SerializeField] private TMP_Text keyText;

        [Tooltip("Optional: the ability's cost (mana, energy, HP...). Empty for free abilities and characters without a resource")]
        [SerializeField] private TMP_Text costText;

        [Tooltip("Cost text colour when the character pays with HP")]
        [SerializeField] private Color healthCostColor = new Color(1f, 0.45f, 0.45f);

        [Tooltip("Optional: shown while the ability is readied or switched on (e.g. a highlighted border)")]
        [SerializeField] private GameObject activeHighlight;

        [Tooltip("Icon colour while the slot is empty")]
        [SerializeField] private Color emptyColor = new Color(1f, 1f, 1f, 0.1f);

        [Tooltip("Icon colour for an ability that has no Icon sprite set yet")]
        [SerializeField] private Color noIconColor = new Color(0.35f, 0.35f, 0.4f, 1f);

        [Tooltip("Multiplied onto the icon while the character can't pay the ability's cost")]
        [SerializeField] private Color unaffordableTint = new Color(0.35f, 0.45f, 0.9f, 1f);

        private AbilityCaster _caster;
        private AbilitySlot _slot;
        private AbilityDefinition _shownAbility;
        private int _shownSeconds = -1;
        private Color _iconColor;
        private bool _shownAffordable = true;
        private string _shownCost;
        private Color _shownCostColor;

        // A filled image needs a sprite to sweep. Unity's default UI sprite has rounded, see-through corners,
        // so a plain white square is made once and shared by every slot.
        private static Sprite _squareSprite;

        private void Awake()
        {
            if (cooldownOverlay != null && cooldownOverlay.sprite == null)
            {
                if (_squareSprite == null)
                {
                    Texture2D white = Texture2D.whiteTexture;
                    _squareSprite = Sprite.Create(white, new Rect(0f, 0f, white.width, white.height), new Vector2(0.5f, 0.5f));
                    _squareSprite.name = "CooldownSquare";
                }

                cooldownOverlay.sprite = _squareSprite;
            }
        }

        public void Bind(AbilityCaster caster, AbilitySlot slot, string keyLabel)
        {
            _caster = caster;
            _slot = slot;
            _shownAbility = null;
            _shownSeconds = -1;

            if (keyText != null)
            {
                keyText.text = keyLabel ?? string.Empty;
            }

            RefreshIcon(null);
        }

        private void Update()
        {
            // The caster is gone (e.g. the player died and was destroyed)
            if (_caster == null)
            {
                ShowCooldown(0f, 0f);
                SetActive(false);
                ShowCost(string.Empty, Color.white);
                return;
            }

            // Polled so swapping or stealing abilities updates the icon on its own
            AbilityDefinition ability = _caster.GetDefinition(_slot);
            if (ability != _shownAbility)
            {
                RefreshIcon(ability);
            }

            ShowCooldown(_caster.GetCooldownFraction(_slot), _caster.GetCooldownRemaining(_slot));
            UpdateCost(ability);

            bool active = _caster.IsActive(_slot);
            SetActive(active);

            // Something already switched on can always be switched off, so don't tint it
            ShowAffordable(active || _caster.CanAfford(_slot));
        }

        private void RefreshIcon(AbilityDefinition ability)
        {
            _shownAbility = ability;

            if (icon == null)
            {
                return;
            }

            bool hasIcon = ability != null && ability.Icon != null;
            icon.sprite = hasIcon ? ability.Icon : null;
            _iconColor = hasIcon ? Color.white : ability != null ? noIconColor : emptyColor;
            icon.color = _shownAffordable ? _iconColor : _iconColor * unaffordableTint;
        }

        private void ShowAffordable(bool affordable)
        {
            // Only touch the colour when it changes (changing it makes the UI redraw)
            if (affordable == _shownAffordable)
            {
                return;
            }

            _shownAffordable = affordable;

            if (icon != null)
            {
                icon.color = affordable ? _iconColor : _iconColor * unaffordableTint;
            }
        }

        private void UpdateCost(AbilityDefinition ability)
        {
            if (costText == null)
            {
                return;
            }

            CharacterResource resource = _caster.Resource;
            ResourceMode mode = resource != null ? resource.Mode : ResourceMode.None;

            bool show = ability != null && ability.Cost > 0f &&
                        (mode == ResourceMode.Health || (mode == ResourceMode.Custom && resource.HasOwnBar));

            if (!show)
            {
                ShowCost(string.Empty, Color.white);
                return;
            }

            // Lightened a bit so dark resource colours stay readable on the icon
            Color color = mode == ResourceMode.Health
                ? healthCostColor
                : Color.Lerp(resource.Definition.Color, Color.white, 0.35f);

            ShowCost(Mathf.CeilToInt(ability.Cost).ToString(), color);
        }

        private void ShowCost(string text, Color color)
        {
            // Only touch the text when it changes (setting TMP text allocates and redraws)
            if (text == _shownCost && color == _shownCostColor)
            {
                return;
            }

            _shownCost = text;
            _shownCostColor = color;
            costText.text = text;
            costText.color = color;
        }

        private void ShowCooldown(float fraction, float secondsLeft)
        {
            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = fraction;
                cooldownOverlay.enabled = fraction > 0f;
            }

            if (cooldownText == null)
            {
                return;
            }

            // Only touch the text when the number changes (setting TMP text every frame allocates)
            int seconds = secondsLeft > 0f ? Mathf.CeilToInt(secondsLeft) : 0;
            if (seconds == _shownSeconds)
            {
                return;
            }

            _shownSeconds = seconds;
            cooldownText.text = seconds > 0 ? seconds.ToString() : string.Empty;
        }

        private void SetActive(bool active)
        {
            if (activeHighlight != null && activeHighlight.activeSelf != active)
            {
                activeHighlight.SetActive(active);
            }
        }
    }
}
