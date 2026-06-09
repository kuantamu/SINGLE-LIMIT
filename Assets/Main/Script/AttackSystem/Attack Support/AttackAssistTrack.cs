using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(1f, 0.35f, 0.1f)]                       // Timeline 上でオレンジ色表示
[TrackClipType(typeof(AttackAssistAsset))]           // このトラックに配置できるクリップ型
[TrackBindingType(typeof(AttackAssistController))]   // バインド対象のコンポーネント型
public class AttackAssistTrack : TrackAsset
{

    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<AttackAssistMixerBehaviour>.Create(graph, inputCount);
    }
}

public class AttackAssistMixerBehaviour : PlayableBehaviour
{
}
