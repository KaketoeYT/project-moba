using UnityEngine;

// What a character spends on abilities.
//   None   - abilities are free
//   Health - abilities cost HP (can't spend your last HP)
//   Custom - its own bar, described by a ResourceDefinition asset (mana, energy, blood...)
namespace _Project.Scripts.Player
{
    public enum ResourceMode
    {
        None = 0,
        Health = 1,
        Custom = 2
    }

    // A kind of resource: make one asset per kind (Mana, Energy, Blood...) and pick it on a character.
    // Only how it looks and starts; how much a character has and how fast it refills is set on the character.
    [CreateAssetMenu(menuName = "MOBA/Resource", fileName = "NewResource")]
    public class ResourceDefinition : ScriptableObject
    {
        [SerializeField] private string displayName;

        [Tooltip("Colour of the bar on the HUD")]
        [SerializeField] private Color color = new Color(0.25f, 0.55f, 1f);

        [Tooltip("How full the bar starts: 1 = full (mana, energy), 0 = empty (rage, blood you build up)")]
        [SerializeField, Range(0f, 1f)] private float startFraction = 1f;

        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public Color Color => color;
        public float StartFraction => startFraction;
    }
}
