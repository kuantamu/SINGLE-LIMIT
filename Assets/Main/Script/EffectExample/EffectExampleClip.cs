using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// Timeline clip for EffectExampleTrack.
/// The prefab ParticleSystem duration is exposed to Timeline.
/// </summary>
[System.Serializable]
public class EffectExampleClip : PlayableAsset, ITimelineClipAsset
{
    private const double DefaultDuration = 1.0;

    [Header("Effect Prefab")]
    [Tooltip("Effect GameObject prefab, such as ParticleSystem or VFX Graph.")]
    public GameObject EffectPrefab;

    [Header("Placement")]
    [Tooltip("Local position.")]
    public Vector3 Position = Vector3.zero;

    [Tooltip("Local Euler rotation.")]
    public Vector3 Rotation = Vector3.zero;

    [Tooltip("Local scale.")]
    public Vector3 Scale = Vector3.one;

    public ClipCaps clipCaps => ClipCaps.Blending | ClipCaps.ClipIn | ClipCaps.SpeedMultiplier;

    public override double duration
    {
        get
        {
            float effectDuration = EffectExampleDurationUtility.GetEffectDuration(EffectPrefab);
            return effectDuration > 0f ? effectDuration : DefaultDuration;
        }
    }

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
