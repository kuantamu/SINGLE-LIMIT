using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


[TrackColor(0.9f, 0.3f, 0.2f)]          // Timeline 上でのトラック色（赤系）
[TrackClipType(typeof(HitBoxClip))]      // このトラックに置けるクリップの型
[TrackBindingType(typeof(Transform))]    // bind 先の型（キャラクターの Transform）
public class HitBoxTrack : TrackAsset
{
    public override Playable CreateTrackMixer(
        PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<HitBoxMixerBehaviour>.Create(graph, inputCount);
    }
}
