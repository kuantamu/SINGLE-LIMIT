using UnityEngine;

/// <summary>
/// 回避の各種設定値。PlayerStateMachine にアタッチして Inspector から調整する。
/// </summary>
[System.Serializable]
public class DodgeSettings
{
    [Tooltip("前隙（秒）：回避開始から移動開始までの硬直時間")]
    public float PreLag = 0.1f;

    [Tooltip("回避時間（秒）：実際に移動する時間")]
    public float ActiveDuration = 0.3f;

    [Tooltip("後隙（秒）：移動終了から操作可能になるまでの硬直時間")]
    public float PostLag = 0.15f;

    [Tooltip("クールダウン（秒）：次の回避が可能になるまでの時間")]
    public float Cooldown = 1.0f;

    [Header("Invincible")]
    [Tooltip("通常回避の無敵時間（秒）")]
    public float InvincibleDuration = 0.25f;

    [Tooltip("連続回避レベル1段階ごとに無敵時間へ掛ける倍率")]
    public float InvinciblePenaltyPerLevel = 0.12f;

    [Tooltip("無敵時間の最小倍率")]
    public float MinInvincibleMultiplier = 0.4f;

    [Header("Repeated Dodge")]
    [Tooltip("この秒数以内に次の回避を出すと連続回避扱い")]
    public float RepeatWindow = 1.0f;

    [Tooltip("回避後、この秒数が経つとペナルティが下がり始める")]
    public float PenaltyDecayDelay = 2.0f;

    [Tooltip("減少開始後、この秒数ごとに1段階下がる")]
    public float PenaltyDecayInterval = 1.5f;

    [Tooltip("回避1回ごとに加算するペナルティ")]
    public float BasePenalty = 0.3f;

    [Tooltip("連続回避時に追加で加算するペナルティ")]
    public float RepeatPenaltyBonus = 0.3f;

    [Tooltip("ペナルティがこの値に達するごとに1段階上がる")]
    public float PenaltyPerLevel = 1.0f;

    [Tooltip("連続回避の最大段階")]
    public int MaxPenaltyLevel = 5;

    [Tooltip("1段階ごとに前隙・後隙・全体時間へ掛ける遅延率")]
    public float LagPenaltyPerLevel = 0.2f;
}
