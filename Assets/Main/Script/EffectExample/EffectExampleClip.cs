using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// EffectExampleTrack に配置するクリップ。
/// Timeline 上でエフェクト Prefab を配置・タイミング制御する。
/// 
/// ■ 設定方法
///   1. Inspector の「Effect Prefab」に ParticleSystem または VFX Prefab を指定
///   2. Position / Rotate / Scale で配置を調整
///   3. クリップの長さでエフェクト再生期間を制御
/// </summary>
[System.Serializable]
public class EffectExampleClip : PlayableAsset, ITimelineClipAsset
{
    [Header("エフェクト Prefab")]
    [Tooltip("ParticleSystem、VFX Graph、またはその他のエフェクト GameObject")]
    public GameObject EffectPrefab;

    [Header("配置設定")]
    [Tooltip("配置位置（ローカル座標）")]
    public Vector3 Position = Vector3.zero;

    [Tooltip("回転（オイラー角）")]
    public Vector3 Rotation = Vector3.zero;

    [Tooltip("スケール")]
    public Vector3 Scale = Vector3.one;

    public ClipCaps clipCaps => ClipCaps.None;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<EffectExampleBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();

        behaviour.EffectPrefab = EffectPrefab;
        behaviour.Position = Position;
        behaviour.Rotation = Rotation;
        behaviour.Scale = Scale;

        return playable;
    }
}
