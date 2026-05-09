using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStatData", menuName = "Stats/CharacterStatData")]
public class CharacterStatData : ScriptableObject
{
    [Header("基本ステータス")]
    [Tooltip("最大 HP")]
    public int MaxHP = 100;

    [Tooltip("攻撃力（プレイヤーは武器側の値を使うため参照されない）")]
    public int AttackPower = 10;

    [Header("クリティカル")]
    [Tooltip("クリティカル率（0〜1）")]
    [Range(0f, 1f)]
    public float CriticalRate = 0.05f;

    [Tooltip("クリティカル発生時のダメージ倍率")]
    public float CriticalMultiplier = 1.5f;

    [Header("属性耐性")]
    public ResistanceLevel SlashResistance  = ResistanceLevel.Normal;
    public ResistanceLevel PierceResistance = ResistanceLevel.Normal;
    public ResistanceLevel StrikeResistance = ResistanceLevel.Normal;

    /// <summary>
    /// 属性に対応する耐性レベルを取得する。
    /// </summary>
    public ResistanceLevel GetAttributeResistanceLevel(AttributeType attributeType)
    {
        return attributeType switch
        {
            AttributeType.Slash  => SlashResistance,
            AttributeType.Pierce => PierceResistance,
            AttributeType.Strike => StrikeResistance,
            _                    => ResistanceLevel.Normal
        };
    }
}
