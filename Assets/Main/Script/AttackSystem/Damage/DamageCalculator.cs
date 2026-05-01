using UnityEngine;

/// <summary>
/// ダメージ計算式をまとめた静的クラス。
///
/// ■ 計算式
///   最終ダメージ = 攻撃力 × 属性倍率 × クリティカル倍率 × 防御倍率
///
/// ■ 属性倍率
///   弱点 → 1.5 / 通常 → 1.0 / 耐性 → 0.5 / 免疫 → 0.0
///
/// ■ クリティカル
///   クリティカル率で抽選し、成功すればクリティカルダメージ倍率を乗算
///
/// ■ 防御アクション中
///   最終ダメージ × 0.5（50%カット）
/// </summary>
public static class DamageCalculator
{
    private const float GuardDamageRate = 0.5f;

    private const float WeakMultiplier   = 1.5f;
    private const float NormalMultiplier = 1.0f;
    private const float ResistMultiplier = 0.5f;
    private const float ImmuneMultiplier = 0.0f;

    /// <summary>
    /// 最終ダメージを計算して返す。
    /// </summary>
    /// <param name="info">攻撃情報</param>
    /// <param name="defenderStats">防御側のステータス</param>
    /// <param name="isCritical">クリティカルが発生したか（out）</param>
    public static int Calculate(
        DamageInfo       info,
        CharacterStatData defenderStats,
        out bool          isCritical)
    {
        // クリティカル抽選
        isCritical = Random.value < info.CriticalRate;

        float skillMultiplier = info.SkillPower;

        // 属性倍率
        float attrMultiplier = GetAttributeMultiplier(info.Attribute, defenderStats);

        // クリティカル倍率
        float critMultiplier = isCritical ? info.CriticalMultiplier : 1.0f;

        // 防御倍率
        float guardMultiplier = info.IsGuarded ? GuardDamageRate : 1.0f;

        float raw = info.AttackPower * attrMultiplier * critMultiplier * guardMultiplier * skillMultiplier;

        return Mathf.Max(0, Mathf.RoundToInt(raw));
    }

    /// <summary>属性と防御側の耐性から倍率を取得する</summary>
    private static float GetAttributeMultiplier(
        AttributeType     attribute,
        CharacterStatData defenderStats)
    {
        ResistanceLevel resistance = attribute switch
        {
            AttributeType.Slash  => defenderStats.SlashResistance,
            AttributeType.Pierce => defenderStats.PierceResistance,
            AttributeType.Strike => defenderStats.StrikeResistance,
            _                    => ResistanceLevel.Normal
        };

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
