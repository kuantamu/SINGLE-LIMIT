using UnityEngine;

/// <summary>
/// ダメージバフの適用対象を示す列挙型。
/// バフが「与えるダメージ」「受けるダメージ」のどちらに影響するかを区別する。
/// </summary>
public enum DamageBuffTarget
{
    /// <summary>自分が敵に与えるダメージへの補正（攻撃バフ）</summary>
    [InspectorName("与ダメージ")]
    OutgoingDamage,

    /// <summary>自分が敵から受けるダメージへの補正（防御バフ／被ダメデバフ）</summary>
    [InspectorName("被ダメージ")]
    IncomingDamage
}
