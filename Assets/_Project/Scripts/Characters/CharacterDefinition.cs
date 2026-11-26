using _Project.Scripts.Abilities;
using UnityEngine;

// A character is only data: stats plus one ability per slot.
// Abilities are just references, so two characters can share the same ability asset.
[CreateAssetMenu(menuName = "MOBA/Character", fileName = "NewCharacter")]
public class CharacterDefinition : ScriptableObject
{
    [SerializeField] private string displayName;
    [SerializeField, TextArea] private string idea;

    [Header("Stats")]
    [SerializeField, Min(0f)] private float moveSpeed = 5f;
    [SerializeField, Min(1f)] private float maxHealth = 100f;

    [Header("Abilities")]
    [SerializeField] private AbilityDefinition basicAttack;
    [SerializeField] private AbilityDefinition passive;
    [SerializeField] private AbilityDefinition ability1;
    [SerializeField] private AbilityDefinition ability2;
    [SerializeField] private AbilityDefinition ability3;
    [SerializeField] private AbilityDefinition ultimate;

    public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
    public string Idea => idea;
    public float MoveSpeed => moveSpeed;
    public float MaxHealth => maxHealth;

    public AbilityDefinition GetAbility(AbilitySlot slot)
    {
        switch (slot)
        {
            case AbilitySlot.BasicAttack: return basicAttack;
            case AbilitySlot.Passive: return passive;
            case AbilitySlot.Ability1: return ability1;
            case AbilitySlot.Ability2: return ability2;
            case AbilitySlot.Ability3: return ability3;
            case AbilitySlot.Ultimate: return ultimate;
            default: return null;
        }
    }
}