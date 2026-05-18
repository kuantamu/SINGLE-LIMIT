using System;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// バフ/デバフの効果量ステータス。
/// ヒットエフェクトなどから参照され、どの種類のバフをどの倍率・時間で付与するかを定義する。
/// Inspector で各攻撃やスキルごとに設定する。
/// </summary>
[Serializable]
public class BuffDebuffApplication
{
    #region SerializeField
    // ---  ---

    /// <summary>
    /// バフの適用対象（与ダメージ強化 or 被ダメージ増加）。
    /// 旧フィールド名 "_target" からリネームされているため FormerlySerializedAs で互換性を保持。
    /// </summary>
    [FormerlySerializedAs("_target")]
    [SerializeField] private DamageBuffTarget _buffType = DamageBuffTarget.IncomingDamage;

    /// <summary>ダメージに掛け算する倍率（例: 1.5 = 1.5倍）</summary>
    [SerializeField] private float _multiplier = 1f;

    /// <summary>バフの持続時間（秒）</summary>
    [SerializeField] private float _duration = 5f;

    /// <summary>同じIDのバフが既に存在する場合、倍率を上書きするか</summary>
    [InspectorName("効果量を上書き"),SerializeField] private bool _overwriteMultiplier = true;

    /// <summary>同じIDのバフが既に存在する場合、持続時間を上書きするか</summary>
    [InspectorName("時間を上書き"), SerializeField] private bool _overwriteDuration = true;

    #endregion

    #region プロパティ
    /// <summary>バフの適用対象（与/被ダメージ）</summary>
    public DamageBuffTarget BuffType => _buffType;

    /// <summary>ダメージ倍率</summary>
    public float Multiplier => _multiplier;

    /// <summary>持続時間（秒）</summary>
    public float Duration => _duration;

    /// <summary>既存バフの倍率を上書きするか</summary>
    public bool OverwriteMultiplier => _overwriteMultiplier;

    /// <summary>既存バフの持続時間を上書きするか</summary>
    public bool OverwriteDuration => _overwriteDuration;

    /// <summary>
    /// バフの識別キー。BuffType の列挙名をそのまま使用することで、
    /// 同種のバフが重複して積み重ならないようにしている。
    /// </summary>
    public string BuffKey => _buffType.ToString();
    #endregion
}
