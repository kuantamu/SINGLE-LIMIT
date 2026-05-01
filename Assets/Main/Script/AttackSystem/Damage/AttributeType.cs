/// <summary>
/// 攻撃の属性種類。武器に設定する。
/// </summary>
public enum AttributeType
{
    Slash,   // 斬撃
    Pierce,  // 貫通
    Strike   // 打撃
}

/// <summary>
/// 属性耐性の段階。キャラクターの耐性設定に使う。
/// </summary>
public enum ResistanceLevel
{
    Weak,    // 弱点  → 1.5倍
    Normal,  // 通常  → 1.0倍
    Resist,  // 耐性  → 0.5倍
    Immune   // 免疫  → 0.0倍
}
