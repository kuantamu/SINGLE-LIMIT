using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Stats/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("武器ステータス")]
    [Tooltip("武器の攻撃力")]
    public int AttackPower = 15;

    [Tooltip("武器の属性")]
    public AttributeType Attribute = AttributeType.Slash;
}
