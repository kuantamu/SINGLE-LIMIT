using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// Timeline 上でエフェクトを再生するトラック。
/// Transform バインドにエフェクトが配置される対象 (キャラクターなど) を設定する。
/// </summary>
[TrackBindingType(typeof(Transform))]
[TrackColor(1, 0, 0)]
[TrackClipType(typeof(EffectExampleClip))]
public class EffectExampleTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        // 標準的な mixer を生成
        return ScriptPlayable<EffectExampleMixerBehaviour>.Create(graph, inputCount);
    }
}
