using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Linq;

/// <summary>
/// 敵の Timeline 再生を管理する。
/// プレイヤーの PlayerAnimationController と同構造。
///
/// ■ 子キャラクターの Animator を自動バインド
/// ■ Signal の種類
///   NotifyAttackActive()  : 攻撃判定を有効にするタイミング
///   NotifyAttackEnd()     : 攻撃判定を無効にするタイミング
///   NotifyMotionEnd 代わりに stopped イベントを使用
/// </summary>
[RequireComponent(typeof(PlayableDirector))]
public class EnemyAnimationController : MonoBehaviour
{
    [Header("Timelines")]
    [SerializeField] private TimelineAsset _idleTimeline;
    [SerializeField] private TimelineAsset _chaseTimeline;
    [SerializeField] private TimelineAsset _staggerTimeline;
    [SerializeField] private TimelineAsset _deathTimeline;

    [Header("Attack Timelines（攻撃パターン順に並べる）")]
    [SerializeField] private TimelineAsset[] _attackTimelines;

    // ---- イベント ----

    /// <summary>Timeline が自然終了した時のイベント</summary>
    public event Action OnMotionEnd;

    /// <summary>
    /// 攻撃判定を有効にするタイミングの通知。
    /// Signal から NotifyAttackActive() を呼ぶよう Inspector で設定する。
    /// </summary>
    public event Action OnAttackActive;

    /// <summary>
    /// 攻撃判定を無効にするタイミングの通知。
    /// Signal から NotifyAttackEnd() を呼ぶよう Inspector で設定する。
    /// </summary>
    public event Action OnAttackEnd;

    // ---- プロパティ ----

    /// <summary>攻撃パターンの総数</summary>
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
            Debug.LogWarning("[EnemyAnimationController] 子オブジェクトに Animator が見つかりません。");
            return;
        }

        _characterAnimator.applyRootMotion = false;

        _director.stopped += _ =>
        {
            if (!_suppressMotionEnd)
                OnMotionEnd?.Invoke();
        };
    }

    // ---- Signal 受信口 ----

    public void NotifyAttackActive() => OnAttackActive?.Invoke();
    public void NotifyAttackEnd()    => OnAttackEnd?.Invoke();

    // ---- 再生 API ----

    public void PlayIdle()    => Play(_idleTimeline,    loop: true);
    public void PlayChase()   => Play(_chaseTimeline,   loop: true);
    public void PlayStagger() => Play(_staggerTimeline, loop: false);
    public void PlayDeath()   => Play(_deathTimeline,  loop: false);

    /// <summary>攻撃パターンインデックスに対応する Timeline を再生する</summary>
    public void PlayAttack(int index)
    {
        if (_attackTimelines == null || _attackTimelines.Length == 0)
        {
            Debug.LogWarning("[EnemyAnimationController] Attack Timelines が設定されていません。");
            return;
        }
        _currentTimeline = null;
        Play(_attackTimelines[index % _attackTimelines.Length], loop: false);
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
