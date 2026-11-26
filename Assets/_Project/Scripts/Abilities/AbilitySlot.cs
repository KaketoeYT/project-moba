using System;

namespace _Project.Scripts.Abilities
{
    // Explicit numbers: the caster uses them as array indexes, so keep them 0, 1, 2... with no gaps
    public enum AbilitySlot
    {
        BasicAttack = 0,
        Passive = 1,
        Ability1 = 2,
        Ability2 = 3,
        Ability3 = 4,
        Ultimate = 5
    }

    public static class AbilitySlots
    {
        // Every slot, computed once (Enum.GetValues allocates, so don't call it every frame)
        public static readonly AbilitySlot[] All = (AbilitySlot[])Enum.GetValues(typeof(AbilitySlot));
    }
}