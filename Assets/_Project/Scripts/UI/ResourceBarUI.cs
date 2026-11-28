using _Project.Scripts.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The bar for a character's resource (mana, energy, blood...), coloured like the resource.
// Hidden when the character has no bar of its own (no resource, or one that costs HP).
// Sits on an object that stays active and switches the `bar` object on and off.
namespace _Project.Scripts.UI
{
    public class ResourceBarUI : MonoBehaviour
    {
        [SerializeField] private CharacterResource target;

        [Tooltip("The whole bar, hidden when there is nothing to show. Must not be this object itself.")]
        [SerializeField] private GameObject bar;

        [Tooltip("The coloured part. It is stretched from the left edge to the current amount.")]
        [SerializeField] private RectTransform fill;

        [Tooltip("Optional: tinted with the resource's colour")]
        [SerializeField] private Graphic fillGraphic;

        [Tooltip("Optional: shows \"40 / 100\"")]
        [SerializeField] private TMP_Text label;

        private ResourceDefinition _shownDefinition;
        private int _shownCurrent = -1;
        private int _shownMax = -1;

        public void SetTarget(CharacterResource resource)
        {
            target = resource;
            _shownDefinition = null;
            _shownCurrent = -1;
        }

        private void Update()
        {
            bool show = target != null && target.HasOwnBar;

            if (bar != null && bar.activeSelf != show)
            {
                bar.SetActive(show);
            }

            if (!show)
            {
                return;
            }

            float current = target.Current;
            float max = target.Max;

            // The fill moves smoothly; colour and text only change when needed
            if (fill != null)
            {
                fill.anchorMin = new Vector2(0f, fill.anchorMin.y);
                fill.anchorMax = new Vector2(Mathf.Clamp01(current / max), fill.anchorMax.y);
            }

            if (target.Definition != _shownDefinition)
            {
                _shownDefinition = target.Definition;

                if (fillGraphic != null)
                {
                    fillGraphic.color = _shownDefinition.Color;
                }
            }

            int currentRounded = Mathf.FloorToInt(current);
            int maxRounded = Mathf.CeilToInt(max);

            if (label != null && (currentRounded != _shownCurrent || maxRounded != _shownMax))
            {
                _shownCurrent = currentRounded;
                _shownMax = maxRounded;
                label.text = $"{currentRounded} / {maxRounded}";
            }
        }
    }
}
