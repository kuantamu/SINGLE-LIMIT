using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public class AttackAssistAsset : PlayableAsset, ITimelineClipAsset
{
    public AttackAssistBehaviour template = new AttackAssistBehaviour();

    public ClipCaps clipCaps => ClipCaps.None;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        ScriptPlayable<AttackAssistBehaviour> playable =
            ScriptPlayable<AttackAssistBehaviour>.Create(graph, template);
        return playable;
    }
}
