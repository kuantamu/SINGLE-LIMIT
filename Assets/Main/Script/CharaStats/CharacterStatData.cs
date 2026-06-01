using UnityEngine;

/// <summary>
/// キャラクターの基本ステータスを定義する ScriptableObject。
/// プレイヤー・敵ごとに個別のアセットを作成し、Inspector から
/// <see cref="CharacterStats"/> の StatData フィールドに設定して使用する。
///
/// ■ 作成方法
///   Assets メニュー → Create → Stats → CharacterStatData
/// </summary>
[CreateAssetMenu(fileName = "CharacterStatData", menuName = "Stats/CharacterStatData")]
public class CharacterStatData : ScriptableObject
{
    // -------------------------------------------------------
    // 基本ステータス
    // -------------------------------------------------------

    [Header("基本ステータス")]

    [Tooltip("最大 HP")]
    public int MaxHP = 100;

    /// <summary>
    /// 基礎攻撃力。
    /// プレイヤーは装備武器側の AttackPower が優先されるため、
    /// このフィールドはプレイヤーキャラクターでは参照されない。
    /// 敵キャラクターのダメージ計算で使用する。
    /// </summary>
    [Tooltip("攻撃力（プレイヤーは武器側の値を使うため参照されない）")]
    public int AttackPower = 10;

    // -------------------------------------------------------
    // クリティカル
    // -------------------------------------------------------

    [Header("クリティカル")]

    /// <summary>クリティカルヒットが発生する確率（0 = 0%、1 = 100%）。</summary>
    [Tooltip("クリティカル率（0〜1）")]
    [Range(0f, 1f)]
    public float CriticalRate = 0.05f;

    /// <summary>
    /// クリティカルヒット時のダメージ倍率。
    /// 例: 1.5 の場合、通常ダメージの 1.5 倍を与える。
    /// </summary>
    [Tooltip("クリティカル発生時のダメージ倍率")]
    public float CriticalMultiplier = 1.5f;

    // -------------------------------------------------------
    // 属性耐性
    // -------------------------------------------------------

    [Header("属性耐性")]

    /// <summary>斬撃属性（Slash）に対する耐性レベル。</summary>
    public ResistanceLevel SlashResistance  = ResistanceLevel.Normal;

    /// <summary>刺突属性（Pierce）に対する耐性レベル。</summary>
    public ResistanceLevel PierceResistance = ResistanceLevel.Normal;

    /// <summary>打撃属性（Strike）に対する耐性レベル。</summary>
    public ResistanceLevel StrikeResistance = ResistanceLevel.Normal;

    // -------------------------------------------------------
    // 公開 API
    // -------------------------------------------------------

    /// <summary>
    /// 指定した属性タイプに対応する耐性レベルを返す。
    /// 定義されていない属性の場合は <see cref="ResistanceLevel.Normal"/> を返す。
    /// </summary>
    /// <param name="attributeType">照会する属性タイプ</param>
    /// <returns>対応する耐性レベル</returns>
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
