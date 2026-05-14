using System;
using UnityEngine;
using UnityEngine.Timeline;

public class PlayerAnimationController : TimelineAnimationControllerBase
{
    [Header("Timelines")]
    [SerializeField] private TimelineAsset _idleTimeline;
    [SerializeField] private TimelineAsset _moveTimeline;
    [SerializeField] private TimelineAsset _guardTimeline;
    [SerializeField] private TimelineAsset _dodgeTimeline;
    [SerializeField] private TimelineAsset _deathTimeline;
    [SerializeField] private TimelineAsset _specialTimeline;
    [SerializeField] private TimelineAsset _staggerTimeline;

    [Header("Attack Timelines")]
    [SerializeField] private TimelineAsset[] _attackTimelines;
    [SerializeField] private TimelineAsset _heavyAttackTimeline;

    public event Action OnBufferOpen;
    public event Action OnCancellableFrame;

    public int AttackCount =>
        (_attackTimelines != null && _attackTimelines.Length > 0)
            ? _attackTimelines.Length
            : 1;

    public void NotifyBufferOpen() => OnBufferOpen?.Invoke();
    public void NotifyCancellableFrame() => OnCancellableFrame?.Invoke();

    public void PlayIdle() => PlayTimeline(_idleTimeline, loop: true);
    public void PlayMove() => PlayTimeline(_moveTimeline, loop: true);
    public void PlayGuard() => PlayTimeline(_guardTimeline, loop: true);
    public void PlayDodge() => PlayTimeline(_dodgeTimeline, loop: false);
    public void PlayDeath() => PlayTimeline(_deathTimeline, loop: false);
    public void PlaySpecial() => PlayTimeline(_specialTimeline, loop: false);
    public void PlayHeavyAttack() => PlayTimeline(_heavyAttackTimeline, loop: false);
    public void PlayStagger() => PlayTimeline(_staggerTimeline, loop: false);

    public void PlayAttack(int comboIndex)
    {
        if (_attackTimelines == null || _attackTimelines.Length == 0)
        {
            Debug.LogWarning("[PlayerAnimationController] Attack Timelines is not configured.");
            return;
        }

        PlayTimeline(_attackTimelines[comboIndex % _attackTimelines.Length], loop: false, forceRestart: true);
    }
}
