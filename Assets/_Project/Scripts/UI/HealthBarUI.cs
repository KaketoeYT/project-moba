using _Project.Scripts.Player;
using TMPro;
using UnityEngine;

// A health bar for a PlayerStats. The fill is any RectTransform inside the bar's background:
// its right edge is moved to match the health, so it works with any image (no sprite needed).
namespace _Project.Scripts.UI
{
    public class HealthBarUI : MonoBehaviour
    {
        [SerializeField] private PlayerStats target;

        [Tooltip("The coloured part of the bar. It is stretched from the left edge to the current health.")]
        [SerializeField] private RectTransform fill;

        [Tooltip("Optional: shows \"75 / 100\"")]
        [SerializeField] private TMP_Text label;

        private float _shownHealth = -1f;
        private float _shownMax = -1f;

        public void SetTarget(PlayerStats stats)
        {
            target = stats;
            _shownHealth = -1f;
        }

        private void Update()
        {
            // Destroyed target (dead) shows an empty bar
            float health = target != null ? target.Health : 0f;
            float max = target != null ? target.MaxHealth : 1f;

            if (Mathf.Approximately(health, _shownHealth) && Mathf.Approximately(max, _shownMax))
            {
                return;
            }

            _shownHealth = health;
            _shownMax = max;

            if (fill != null)
            {
                fill.anchorMin = new Vector2(0f, fill.anchorMin.y);
                fill.anchorMax = new Vector2(Mathf.Clamp01(health / max), fill.anchorMax.y);
            }

            if (label != null)
            {
                label.text = $"{Mathf.CeilToInt(health)} / {Mathf.CeilToInt(max)}";
            }
        }
    }
}
