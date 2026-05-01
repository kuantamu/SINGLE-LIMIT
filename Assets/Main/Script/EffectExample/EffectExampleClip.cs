using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class EffectExampleClip : PlayableAsset, ITimelineClipAsset
{
    public EffectExampleBehaviour effectExample = new EffectExampleBehaviour();


    // このクリップの特徴を定義
    public ClipCaps clipCaps
    {
        get
        {
            // ブレンドに対応、タイムスケール変更に対応
            return ClipCaps.Blending | ClipCaps.SpeedMultiplier;
        }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        // PlayableBehaviourを元にPlayableを作る
        var playable = ScriptPlayable<EffectPlayableBehaviour>.Create(graph);
        // PlayableBehaviourを取得する
        var behaviour = playable.GetBehaviour();
        behaviour.particleSystem = effectExample.ParticleObj;
        behaviour.Example_tf = effectExample;
        // BehaviourのPlayableを作って返すだけ
        return playable;
    }
}
