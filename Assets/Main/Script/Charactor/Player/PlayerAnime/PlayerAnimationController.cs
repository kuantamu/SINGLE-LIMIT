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

    private WeaponHolder _weaponHolder;

    public event Action OnBufferOpen;
    public event Action OnCancellableFrame;

    public int AttackCount =>
        (CurrentAnimations?.Attacks != null && CurrentAnimations.Attacks.Length > 0)
            ? CurrentAnimations.Attacks.Length
            : (_attackTimelines != null && _attackTimelines.Length > 0)
                ? _attackTimelines.Length
                : 1;

    private WeaponAnimationSet CurrentAnimations => _weaponHolder != null
        ? _weaponHolder.CurrentWeapon?.Animations
        : null;

    protected override void Awake()
    {
        base.Awake();
        _weaponHolder = GetComponentInParent<WeaponHolder>();
    }

    private void OnEnable()
    {
        if (_weaponHolder != null)
            _weaponHolder.OnWeaponChanged += HandleWeaponChanged;
    }

    private void OnDisable()
    {
        if (_weaponHolder != null)
            _weaponHolder.OnWeaponChanged -= HandleWeaponChanged;
    }

    private void HandleWeaponChanged(WeaponData weapon)
    {
        PlayIdle();
    }

    public void NotifyBufferOpen() => OnBufferOpen?.Invoke();
    public void NotifyCancellableFrame() => OnCancellableFrame?.Invoke();

    public void PlayIdle() => PlayTimeline(ResolveTimeline(CurrentAnimations?.Idle, _idleTimeline), loop: true);
    public void PlayMove() => PlayTimeline(ResolveTimeline(CurrentAnimations?.Move, _moveTimeline), loop: true);
    public void PlayGuard() => PlayTimeline(ResolveTimeline(CurrentAnimations?.Guard, _guardTimeline), loop: true);
    public void PlayDodge() => PlayTimeline(ResolveTimeline(CurrentAnimations?.Dodge, _dodgeTimeline), loop: false);
    public void PlayDeath() => PlayTimeline(ResolveTimeline(CurrentAnimations?.Death, _deathTimeline), loop: false);
    public void PlaySpecial() => PlayTimeline(ResolveTimeline(CurrentAnimations?.Special, _specialTimeline), loop: false);
    public void PlayHeavyAttack() => PlayTimeline(ResolveTimeline(CurrentAnimations?.HeavyAttack, _heavyAttackTimeline), loop: false);
    public void PlayStagger() => PlayTimeline(ResolveTimeline(CurrentAnimations?.Stagger, _staggerTimeline), loop: false);

    public void PlayAttack(int comboIndex)
    {
        TimelineAsset[] timelines = ResolveAttackTimelines();
        if (timelines == null || timelines.Length == 0)
        {
            Debug.LogWarning("[PlayerAnimationController] Attack timelines are not configured.");
            return;
        }

        PlayTimeline(timelines[comboIndex % timelines.Length], loop: false, forceRestart: true);
    }

    private TimelineAsset ResolveTimeline(TimelineAsset weaponTimeline, TimelineAsset defaultTimeline)
    {
        return weaponTimeline != null ? weaponTimeline : defaultTimeline;
    }

    private TimelineAsset[] ResolveAttackTimelines()
    {
        if (CurrentAnimations?.Attacks != null && CurrentAnimations.Attacks.Length > 0)
            return CurrentAnimations.Attacks;

        return _attackTimelines;
    }
}
