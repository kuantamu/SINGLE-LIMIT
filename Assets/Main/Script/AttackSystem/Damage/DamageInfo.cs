/// <summary>
/// ダメージ計算に必要な情報をまとめた構造体。
/// HitBoxClip から DamageHitEffect へ渡す。
/// </summary>
public struct DamageInfo
{
    /// <summary>攻撃側の攻撃力</summary>
    public int AttackPower;

    /// <summary>攻撃の属性</summary>
    public AttributeType Attribute;

    /// <summary>攻撃側のクリティカル率（0〜1）</summary>
    public float CriticalRate;

    /// <summary>攻撃側のクリティカルダメージ倍率</summary>
    public float CriticalMultiplier;

    /// <summary>防御アクション中のダメージかどうか</summary>
    public bool IsGuarded;

    /// <summary>スキル倍率</summary>
    public float SkillPower;
}
