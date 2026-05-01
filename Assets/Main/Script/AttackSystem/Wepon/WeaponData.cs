using UnityEngine;

/// <summary>
/// 武器のステータスを保持する ScriptableObject。
/// Assets を右クリック → Create → Stats → WeaponData で作成する。
///
/// ■ 攻撃力と属性は武器全体で統一される。
///   コンボの各段で属性が変わる場合は別の武器データを作成する。
/// </summary>
[CreateAssetMenu(fileName = "WeaponData", menuName = "Stats/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("武器ステータス")]
    [Tooltip("武器の攻撃力")]
    public int AttackPower = 15;

    [Tooltip("武器の属性")]
    public AttributeType Attribute = AttributeType.Slash;
}
