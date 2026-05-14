using System;
using UnityEngine;
using UnityEngine.Timeline;

public class EnemyAnimationController : TimelineAnimationControllerBase
{
    [Header("Timelines")]
    [SerializeField] private TimelineAsset _idleTimeline;
    [SerializeField] private TimelineAsset _chaseTimeline;
    [SerializeField] private TimelineAsset _staggerTimeline;
    [SerializeField] private TimelineAsset _deathTimeline;

    [Header("Attack Timelines")]
    [SerializeField] private TimelineAsset[] _attackTimelines;

    public event Action OnAttackActive;
    public event Action OnAttackEnd;

    public int AttackCount =>
        (_attackTimelines != null && _attackTimelines.Length > 0)
            ? _attackTimelines.Length
            : 1;

    public void NotifyAttackActive() => OnAttackActive?.Invoke();
    public void NotifyAttackEnd() => OnAttackEnd?.Invoke();

    public void PlayIdle() => PlayTimeline(_idleTimeline, loop: true);
    public void PlayChase() => PlayTimeline(_chaseTimeline, loop: true);
    public void PlayStagger() => PlayTimeline(_staggerTimeline, loop: false);
    public void PlayDeath() => PlayTimeline(_deathTimeline, loop: false);

    public void PlayAttack(int index)
    {
        if (_attackTimelines == null || _attackTimelines.Length == 0)
        {
            Debug.LogWarning("[EnemyAnimationController] Attack Timelines is not configured.");
            return;
        }

        PlayTimeline(_attackTimelines[index % _attackTimelines.Length], loop: false, forceRestart: true);
    }

    protected override void BindTracks(TimelineAsset asset)
    {
        base.BindTracks(asset);

        foreach (TrackAsset track in asset.GetOutputTracks())
        {
            if (track is HitBoxTrack)
                Director.SetGenericBinding(track, transform);
        }
    }
}
