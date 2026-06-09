using System;
using UnityEngine;

public enum CharacterFaction
{
    Player,

    Enemy,

    Ally,

    Other
}

[Flags]
public enum CharacterFactionMask
{
    None   = 0,

    Player = 1 << 0,

    Enemy  = 1 << 1,

    Ally   = 1 << 2,

    Other  = 1 << 3,

    All    = Player | Enemy | Ally | Other
}

public static class CharacterFactionRules
{
    public static CharacterFactionMask ToMask(CharacterFaction faction)
    {
        return faction switch
        {
            CharacterFaction.Player => CharacterFactionMask.Player,
            CharacterFaction.Enemy  => CharacterFactionMask.Enemy,
            CharacterFaction.Ally   => CharacterFactionMask.Ally,
            CharacterFaction.Other  => CharacterFactionMask.Other,
            _                       => CharacterFactionMask.None
        };
    }

    public static CharacterFactionMask DefaultHostileFactions(CharacterFaction faction)
    {
        return faction switch
        {
            CharacterFaction.Player => CharacterFactionMask.Enemy,
            CharacterFaction.Enemy  => CharacterFactionMask.Player | CharacterFactionMask.Ally,
            CharacterFaction.Ally   => CharacterFactionMask.Enemy,
            CharacterFaction.Other  => CharacterFactionMask.None,
            _                       => CharacterFactionMask.None
        };
    }

    public static CharacterFactionMask DefaultAttackableFactions(CharacterFaction faction)
    {
        return DefaultHostileFactions(faction);
    }

    public static bool Contains(this CharacterFactionMask mask, CharacterFaction faction)
    {
        return (mask & ToMask(faction)) != 0;
    }

    public static CharacterStats GetCharacterStats(Component component)
    {
        return component != null ? component.GetComponentInParent<CharacterStats>() : null;
    }

    public static CharacterStats GetCharacterStats(Transform transform)
    {
        return transform != null ? transform.GetComponentInParent<CharacterStats>() : null;
    }

    public static bool IsHostile(CharacterStats source, CharacterStats target)
    {
        return source != null
            && target != null
            && source != target
            && !target.IsDead
            && source.HostileFactions.Contains(target.Faction);
    }

    public static bool CanAttack(CharacterStats attacker, CharacterStats defender)
    {
        return attacker != null
            && defender != null
            && attacker != defender
            && !defender.IsDead
            && attacker.AttackableFactions.Contains(defender.Faction);
    }
}
