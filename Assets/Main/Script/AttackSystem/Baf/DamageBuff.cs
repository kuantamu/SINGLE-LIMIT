using System;
using UnityEngine;

/// <summary>
/// 単一のダメージバフ／デバフを表すデータクラス。
/// IDと対象（与/被ダメージ）、倍率、持続時間を保持し、毎フレームTickで残り時間を管理する。
/// </summary>
[Serializable]
public class DamageBuff
{
    // --- シリアライズフィールド ---

    /// <summary>バフを一意に識別するキー文字列（同一IDのバフは上書き対象になる）</summary>
    [SerializeField] private string _id;

    /// <summary>バフが影響するダメージの方向（OutgoingDamage=与ダメ / IncomingDamage=被ダメ）</summary>
    [SerializeField] private DamageBuffTarget _target;

    /// <summary>ダメージに掛け算する倍率（1.0 = 等倍、2.0 = 2倍ダメージ など）</summary>
    [SerializeField] private float _multiplier = 1f;

    /// <summary>持続時間（秒）。負値の場合は永続バフとして扱われる</summary>
    [SerializeField] private float _duration = -1f;

    // --- プライベートフィールド ---

    /// <summary>残り持続時間（秒）。Tickによって減算される</summary>
    private float _remainingTime;

    // --- プロパティ ---

    /// <summary>バフの識別ID</summary>
    public string Id => _id;

    /// <summary>バフが影響するダメージ対象</summary>
    public DamageBuffTarget Target => _target;

    /// <summary>ダメージ倍率（最低値0でマイナスにはならない）</summary>
    public float Multiplier => Mathf.Max(0f, _multiplier);

    /// <summary>設定された持続時間（秒）</summary>
    public float Duration => _duration;

    /// <summary>残り持続時間（秒）</summary>
    public float RemainingTime => _remainingTime;

    /// <summary>duration が負値のとき永続バフと判定される</summary>
    public bool IsPermanent => _duration < 0f;

    /// <summary>永続でなく、かつ残り時間が0以下になったとき期限切れと判定される</summary>
    public bool IsExpired => !IsPermanent && _remainingTime <= 0f;

    // --- コンストラクタ ---

    /// <summary>
    /// 新しいダメージバフを生成する。
    /// </summary>
    /// <param name="id">バフの識別キー</param>
    /// <param name="target">与ダメージ／被ダメージのどちらに作用するか</param>
    /// <param name="multiplier">ダメージ倍率</param>
    /// <param name="duration">持続時間（秒）。省略または負値で永続</param>
    public DamageBuff(string id, DamageBuffTarget target, float multiplier, float duration = -1f)
    {
        _id = id;
        _target = target;
        _multiplier = multiplier;
        _duration = duration;
        _remainingTime = duration;
    }

    // --- パブリックメソッド ---

    /// <summary>
    /// 既存バフの倍率と持続時間を更新する（スタック防止の上書きリフレッシュ）。
    /// overwrite フラグが false の場合は対応する値を変更しない。
    /// </summary>
    /// <param name="multiplier">新しい倍率</param>
    /// <param name="duration">新しい持続時間（秒）</param>
    /// <param name="overwriteMultiplier">true なら倍率を上書きする</param>
    /// <param name="overwriteDuration">true なら持続時間を上書きし残り時間もリセットする</param>
    public void Refresh(
        float multiplier,
        float duration,
        bool overwriteMultiplier = true,
        bool overwriteDuration = true)
    {
        if (overwriteMultiplier)
            _multiplier = multiplier;

        if (!overwriteDuration) return;

        _duration = duration;
        _remainingTime = duration;
    }

    /// <summary>
    /// 毎フレーム呼び出し、残り時間を減算する。
    /// 永続バフの場合は何もしない。
    /// </summary>
    /// <param name="deltaTime">前フレームからの経過時間（通常 Time.deltaTime）</param>
    public void Tick(float deltaTime)
    {
        if (IsPermanent) return;
        _remainingTime -= deltaTime;
    }
}
