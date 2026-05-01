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
}
