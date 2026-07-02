using UnityEngine;

public static class DamageCalculator
{
    private const float GuardDamageRate = 0.5f;

    private const float WeakMultiplier   = 1.5f;
    private const float NormalMultiplier = 1.0f;
    private const float ResistMultiplier = 0.5f;
    private const float ImmuneMultiplier = 0.0f;

    public static int Calculate(DamageInfo info,CharacterStatData defenderStats,out bool isCritical)
    {
        isCritical = Random.value < info.CriticalRate;

        float skillMultiplier = 1.0f - info.SkillPower;
        float outgoingMultiplier = 1.0f;
        if (info.UseOutgoingDamageMultiplier)
        {
            outgoingMultiplier = Mathf.Max(0f, info.OutgoingDamageMultiplier) - outgoingMultiplier;
        }
        float incomingMultiplier = 1.0f;
        if (info.UseIncomingDamageMultiplier)
        {
            incomingMultiplier = Mathf.Max(0f, info.IncomingDamageMultiplier) - incomingMultiplier;
        }
        float attrMultiplier;
        if (info.ForceWeakAttribute)
        {
            attrMultiplier = WeakMultiplier;
        }
        else
        {
            attrMultiplier = GetAttributeMultiplier(info.Attribute, defenderStats);
        }
        float critMultiplier = 1.0f;
        if (isCritical)
        {
            critMultiplier = info.CriticalMultiplier;
        }
        float guardMultiplier = 1.0f;
        if (info.IsGuarded)
        {
            guardMultiplier = GuardDamageRate;
        }
        float raw = info.AttackPower 
            * attrMultiplier
            * critMultiplier
            * (1.0f + skillMultiplier + outgoingMultiplier + incomingMultiplier)
            * guardMultiplier;

        return Mathf.Max(0, Mathf.RoundToInt(raw));
    }

    private static float GetAttributeMultiplier(
        AttributeType     attribute,
        CharacterStatData defenderStats)
    {
        ResistanceLevel resistance = defenderStats.GetAttributeResistanceLevel(attribute);

        return resistance switch
        {
            ResistanceLevel.Weak   => WeakMultiplier,
            ResistanceLevel.Normal => NormalMultiplier,
            ResistanceLevel.Resist => ResistMultiplier,
            ResistanceLevel.Immune => ImmuneMultiplier,
            _                      => NormalMultiplier
        };
    }
}
