using UnityEngine;

/// <summary>
/// ノックバックの設定。HitBoxClip に持たせて Inspector から調整する。
/// </summary>
[System.Serializable]
public class KnockbackSettings
{
    [Tooltip("ノックバックの移動距離（m）")]
    public float Distance = 2f;

    [Tooltip("ノックバックの移動時間（秒）。距離 ÷ 時間 = 速度で計算する")]
    public float Duration = 0.3f;
}
