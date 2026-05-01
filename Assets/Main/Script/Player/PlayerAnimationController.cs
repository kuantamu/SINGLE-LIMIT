using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Linq;

/// <summary>
/// PlayableDirector を通じて Timeline を再生する。
///
/// ■ 攻撃 Timeline に設定する Signal は2種類
///   NotifyBufferOpen()      : この時点からバッファへの積み込みを許可する
///   NotifyCancellableFrame(): この時点からキャンセル行動（移動・防御）を許可する
///
/// ■ Signal の設定手順
///   1. 攻撃 Timeline に SignalTrack を追加
///   2. バッファ開放タイミングに SignalEmitter を置き
///      SignalReceiver で NotifyBufferOpen() を紐付ける
///   3. キャンセル可能タイミングに SignalEmitter を置き
///      SignalReceiver で NotifyCancellableFrame() を紐付ける
/// </summary>
[RequireComponent(typeof(PlayableDirector))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("Timelines")]
    [SerializeField] private TimelineAsset _idleTimeline;
    [SerializeField] private TimelineAsset _moveTimeline;
    [SerializeField] private TimelineAsset _guardTimeline;
    [SerializeField] private TimelineAsset _dodgeTimeline;
    [SerializeField] private TimelineAsset _deathTimeline;
    [SerializeField] private TimelineAsset _specialTimeline;

    [Header("Attack Timelines（コンボ順に並べる）")]
    [SerializeField] private TimelineAsset[] _attackTimelines;
    [SerializeField] private TimelineAsset _heavyAttackTimeline;

    #region タイムライン用イベント
    /// <summary>Timeline が自然終了した時のイベント</summary>
    public event Action OnMotionEnd;

    /// <summary>バッファへの積み込みを許可するタイミングの通知</summary>
    public event Action OnBufferOpen;

    /// <summary>キャンセル行動（移動・防御）を許可するタイミングの通知</summary>
    public event Action OnCancellableFrame;

    /** 攻撃判定を有効にするタイミングの通知 */
    public event Action OnAttackActive;

    /** 攻撃判定を無効にするタイミングの通知 */
    public event Action OnAttackEnd;
    #endregion

    public int AttackCount =>
        (_attackTimelines != null && _attackTimelines.Length > 0)
        ? _attackTimelines.Length : 1;

    // ---- 内部 ----
    private PlayableDirector _director;
    private TimelineAsset    _currentTimeline;
    private Animator         _characterAnimator;
    private bool             _suppressMotionEnd;

    private void Awake()
    {
        _director = GetComponent<PlayableDirector>();

        _characterAnimator = GetComponentsInChildren<Animator>()
            .FirstOrDefault(a => a.gameObject != gameObject);

        if (_characterAnimator == null)
        {
            Debug.LogWarning("[PlayerAnimationController] 子オブジェクトに Animator が見つかりません。");
            return;
        }

        _characterAnimator.applyRootMotion = false;

        _director.stopped += _ =>
        {
            if (!_suppressMotionEnd)
                OnMotionEnd?.Invoke();
        };

        Debug.Log($"[PlayerAnimationController] Animator を自動バインドしました: {_characterAnimator.gameObject.name}");
    }


    #region シグナル関連
    /// <summary>バッファ開放 Signal の受信口</summary>
    public void NotifyBufferOpen() => OnBufferOpen?.Invoke();

    /// <summary>キャンセル可能フレーム Signal の受信口</summary>
    public void NotifyCancellableFrame() => OnCancellableFrame?.Invoke();
    public void NotifyAttackActive()     => OnAttackActive?.Invoke();
    public void NotifyAttackEnd()        => OnAttackEnd?.Invoke();
    #endregion

    #region 再生API
    public void PlayIdle()  => Play(_idleTimeline,  loop: true);
    public void PlayMove()  => Play(_moveTimeline,  loop: true);
    public void PlayGuard() => Play(_guardTimeline, loop: true);
    public void PlayDodge() => Play(_dodgeTimeline, loop: false);
    public void PlayDeath() => Play(_deathTimeline, loop: false);
    public void PlaySpecial() => Play(_specialTimeline, loop: false);
    public void PlayHeavyAttack() => Play(_heavyAttackTimeline, loop: false);
    #endregion

    public void PlayAttack(int comboIndex)
    {
        if (_attackTimelines == null || _attackTimelines.Length == 0)
        {
            Debug.LogWarning("[PlayerAnimationController] Attack Timelines が設定されていません。");
            return;
        }
        _currentTimeline = null; // 同一インデックス折り返し対応
        Play(_attackTimelines[comboIndex % _attackTimelines.Length], loop: false);
    }

    // ---- 内部 ----

    private void Play(TimelineAsset asset, bool loop)
    {
        if (asset == null || asset == _currentTimeline) return;

        _suppressMotionEnd = true;
        _director.Stop();
        _suppressMotionEnd = false;

        _currentTimeline            = asset;
        _director.playableAsset     = asset;
        _director.extrapolationMode = loop ? DirectorWrapMode.Loop : DirectorWrapMode.None;

        BindAnimatorToAllTracks(asset);
        _director.Play();
    }

    private void BindAnimatorToAllTracks(TimelineAsset asset)
    {
        if (_characterAnimator == null) return;

        foreach (TrackAsset track in asset.GetOutputTracks())
        {
            if (track is AnimationTrack)
                _director.SetGenericBinding(track, _characterAnimator);
        }
    }
}
