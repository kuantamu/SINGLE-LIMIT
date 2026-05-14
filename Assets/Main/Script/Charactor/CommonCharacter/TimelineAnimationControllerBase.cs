using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[RequireComponent(typeof(PlayableDirector))]
public abstract class TimelineAnimationControllerBase : MonoBehaviour
{
    public event Action OnMotionEnd;

    protected PlayableDirector Director { get; private set; }
    protected Animator CharacterAnimator { get; private set; }

    private TimelineAsset _currentTimeline;
    private bool _suppressMotionEnd;

    protected virtual void Awake()
    {
        Director = GetComponent<PlayableDirector>();
        CharacterAnimator = GetComponentsInChildren<Animator>()
            .FirstOrDefault(a => a.gameObject != gameObject);

        if (CharacterAnimator == null)
        {
            Debug.LogWarning($"[{GetType().Name}] Child Animator was not found.");
            return;
        }

        CharacterAnimator.applyRootMotion = false;
        Director.stopped += HandleDirectorStopped;
    }

    protected virtual void OnDestroy()
    {
        if (Director != null)
            Director.stopped -= HandleDirectorStopped;
    }

    protected void PlayTimeline(TimelineAsset asset, bool loop, bool forceRestart = false)
    {
        if (asset == null) return;
        if (!forceRestart && asset == _currentTimeline) return;

        _suppressMotionEnd = true;
        Director.Stop();
        _suppressMotionEnd = false;

        _currentTimeline = asset;
        Director.playableAsset = asset;
        Director.extrapolationMode = loop ? DirectorWrapMode.Loop : DirectorWrapMode.None;

        BindTracks(asset);
        Director.Play();
    }

    protected virtual void BindTracks(TimelineAsset asset)
    {
        if (CharacterAnimator == null) return;

        foreach (TrackAsset track in asset.GetOutputTracks())
        {
            if (track is AnimationTrack)
                Director.SetGenericBinding(track, CharacterAnimator);
        }
    }

    private void HandleDirectorStopped(PlayableDirector director)
    {
        if (!_suppressMotionEnd)
            OnMotionEnd?.Invoke();
    }
}
