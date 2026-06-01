using System;
using UnityEngine;

/// <summary>
/// キャラクターの所属陣営を表す列挙型。
/// 戦闘における敵味方判定や AI の行動目標選択などに使用する。
/// </summary>
public enum CharacterFaction
{
    /// <summary>プレイヤーキャラクター</summary>
    Player,

    /// <summary>敵キャラクター</summary>
    Enemy,

    /// <summary>味方キャラクター（プレイヤーに同行する NPC など）</summary>
    Ally,

    /// <summary>いずれの陣営にも属さない中立キャラクター</summary>
    Other
}

/// <summary>
/// 複数の陣営を同時に表現するためのビットフラグ列挙型。
/// 敵対陣営・攻撃可能陣営のセット指定などに使用する。
/// </summary>
[Flags]
public enum CharacterFactionMask
{
    /// <summary>陣営なし（未設定）</summary>
    None   = 0,

    /// <summary>プレイヤー陣営</summary>
    Player = 1 << 0,

    /// <summary>敵陣営</summary>
    Enemy  = 1 << 1,

    /// <summary>味方陣営</summary>
    Ally   = 1 << 2,

    /// <summary>中立陣営</summary>
    Other  = 1 << 3,

    /// <summary>全陣営（Player | Enemy | Ally | Other）</summary>
    All    = Player | Enemy | Ally | Other
}

/// <summary>
/// 陣営に関するルール・ユーティリティをまとめた静的クラス。
/// 陣営マスクへの変換や、デフォルトの敵対関係定義を提供する。
/// </summary>
public static class CharacterFactionRules
{
    /// <summary>
    /// <see cref="CharacterFaction"/> を対応する単一ビットの
    /// <see cref="CharacterFactionMask"/> へ変換する。
    /// </summary>
    /// <param name="faction">変換元の陣営</param>
    /// <returns>対応するマスク値。未定義の陣営は <see cref="CharacterFactionMask.None"/> を返す。</returns>
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

    /// <summary>
    /// 指定した陣営がデフォルトで敵対する陣営のマスクを返す。
    /// <list type="bullet">
    ///   <item>Player ↔ Enemy が互いに敵対</item>
    ///   <item>Enemy は Ally にも敵対</item>
    ///   <item>Other はいずれにも敵対しない</item>
    /// </list>
    /// </summary>
    /// <param name="faction">陣営</param>
    /// <returns>敵対する陣営の集合を表すマスク</returns>
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

    /// <summary>
    /// 指定した陣営がデフォルトで攻撃可能な陣営のマスクを返す。
    /// 現状は <see cref="DefaultHostileFactions"/> と同一。
    /// 将来的に「敵対はしているが攻撃できない」ケースが生じた際は分離する。
    /// </summary>
    /// <param name="faction">陣営</param>
    /// <returns>攻撃可能な陣営の集合を表すマスク</returns>
    public static CharacterFactionMask DefaultAttackableFactions(CharacterFaction faction)
    {
        return DefaultHostileFactions(faction);
    }

    /// <summary>
    /// マスクが指定した陣営を含むかどうかを判定する拡張メソッド。
    /// </summary>
    /// <param name="mask">検査対象のマスク</param>
    /// <param name="faction">含まれているか調べる陣営</param>
    /// <returns>含まれていれば <c>true</c></returns>
    public static bool Contains(this CharacterFactionMask mask, CharacterFaction faction)
    {
        return (mask & ToMask(faction)) != 0;
    }

    /// <summary>
    /// Component の親階層から <see cref="CharacterStats"/> を取得するユーティリティ。
    /// ヒットボックスなど子オブジェクトの Component からルートの Stats を参照する際に使用する。
    /// </summary>
    /// <param name="component">検索起点となる Component</param>
    /// <returns>見つかった <see cref="CharacterStats"/>。component が null または見つからない場合は null。</returns>
    public static CharacterStats GetCharacterStats(Component component)
    {
        return component != null ? component.GetComponentInParent<CharacterStats>() : null;
    }

    /// <summary>
    /// Transform の親階層から <see cref="CharacterStats"/> を取得するユーティリティ。
    /// </summary>
    /// <param name="transform">検索起点となる Transform</param>
    /// <returns>見つかった <see cref="CharacterStats"/>。transform が null または見つからない場合は null。</returns>
    public static CharacterStats GetCharacterStats(Transform transform)
    {
        return transform != null ? transform.GetComponentInParent<CharacterStats>() : null;
    }

    /// <summary>
    /// source が target に対して敵対しているかを判定する。
    /// 以下の条件をすべて満たす場合に <c>true</c> を返す。
    /// <list type="number">
    ///   <item>どちらも null でない</item>
    ///   <item>同一オブジェクトではない</item>
    ///   <item>target が生存中である</item>
    ///   <item>target の陣営が source の敵対陣営マスクに含まれている</item>
    /// </list>
    /// </summary>
    /// <param name="source">判定を行う側のキャラクター</param>
    /// <param name="target">判定対象のキャラクター</param>
    /// <returns>敵対している場合は <c>true</c></returns>
    public static bool IsHostile(CharacterStats source, CharacterStats target)
    {
        return source != null
            && target != null
            && source != target
            && !target.IsDead
            && source.HostileFactions.Contains(target.Faction);
    }

    /// <summary>
    /// attacker が defender を攻撃できるかを判定する。
    /// <see cref="IsHostile"/> と条件は同様だが、攻撃可能陣営マスク (<see cref="CharacterStats.AttackableFactions"/>) を参照する。
    /// </summary>
    /// <param name="attacker">攻撃側のキャラクター</param>
    /// <param name="defender">防御側のキャラクター</param>
    /// <returns>攻撃可能な場合は <c>true</c></returns>
    public static bool CanAttack(CharacterStats attacker, CharacterStats defender)
    {
        return attacker != null
            && defender != null
            && attacker != defender
            && !defender.IsDead
            && attacker.AttackableFactions.Contains(defender.Faction);
    }
}
