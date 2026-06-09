using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackBindingType(typeof(Transform))]
[TrackColor(1, 0, 0)]
[TrackClipType(typeof(EffectExampleClip))]
public class EffectExampleTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<EffectExampleMixerBehaviour>.Create(graph, inputCount);
    }
}
