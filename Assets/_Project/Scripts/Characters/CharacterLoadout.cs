using UnityEngine;

// "Become this character": applies a CharacterDefinition's stats and abilities to this object.
[RequireComponent(typeof(AbilityCaster))]
public class CharacterLoadout : MonoBehaviour
{
    [SerializeField] private CharacterDefinition character;
    [SerializeField] private AbilityCaster caster;
    [SerializeField] private PlayerStats playerStats;

    public CharacterDefinition Character => character;

    // Called by Unity when you add the component in the editor: auto-fills the references
    private void Reset()
    {
        caster = GetComponent<AbilityCaster>();
        playerStats = GetComponent<PlayerStats>();
    }

    private void Start()
    {
        // Start (not Awake) so the other components have finished setting themselves up
        if (character != null)
        {
            SetCharacter(character);
        }
    }

    // Can also be called at runtime (character select, swap gamemodes...)
    public void SetCharacter(CharacterDefinition definition)
    {
        character = definition;

        if (playerStats != null)
        {
            playerStats.SetBaseStats(definition.MoveSpeed, definition.MaxHealth);
        }

        foreach (AbilitySlot slot in AbilitySlots.All)
        {
            caster.Equip(slot, definition.GetAbility(slot));
        }
    }
}