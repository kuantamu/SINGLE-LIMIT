using UnityEngine;

/// <summary>
/// 敵の状態機械。
/// </summary>
[RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(EnemyAnimationController))]
[RequireComponent(typeof(EnemyDetector))]
[RequireComponent(typeof(Rigidbody))]
public class EnemyStateMachine : MonoBehaviour
{
    // ---- 参照 ----
    public EnemyMovement            Movement       { get; private set; }
    public EnemyAnimationController AnimController { get; private set; }
    public EnemyDetector            Detector       { get; private set; }

    // ---- 設定 ----
    [Header("攻撃範囲")]
    [SerializeField] private float _attackRange = 2f;

    [Header("攻撃インターバル（秒）")]
    [SerializeField] private float _attackInterval = 2f;

    // ---- ステートインスタンス ----
    public EnemyIdleState    Idle    { get; private set; }
    public EnemyChaseState   Chase   { get; private set; }
    public EnemyAttackState  Attack  { get; private set; }
    public EnemyStaggerState Stagger { get; private set; }
    public EnemyDeathState      Death     { get; private set; }
    public EnemyKnockbackState Knockback { get; private set; }

    // ---- 現在のステート ----
    public EnemyState CurrentState { get; private set; }

    // ---- プロパティ ----
    public float AttackRange    => _attackRange;
    public float AttackInterval => _attackInterval;

    /// <summary>検知中のプレイヤー Transform（未検知なら null）</summary>
    public Transform PlayerTransform => Detector.DetectedPlayer;

    /// <summary>プレイヤーまでの水平距離</summary>
    public float DistanceToPlayer
    {
        get
        {
            if (PlayerTransform == null) return float.MaxValue;
            Vector3 diff = PlayerTransform.position - transform.position;
            diff.y = 0f;
            return diff.magnitude;
        }
    }

    // ---- Unity ライフサイクル ----

    private void Awake()
    {
        Movement       = GetComponent<EnemyMovement>();
        AnimController = GetComponent<EnemyAnimationController>();
        Detector       = GetComponent<EnemyDetector>();

        Idle    = new EnemyIdleState(this);
        Chase   = new EnemyChaseState(this);
        Attack  = new EnemyAttackState(this);
        Stagger = new EnemyStaggerState(this);
        Death     = new EnemyDeathState(this);
        Knockback = new EnemyKnockbackState(this);
    }

    private void Start() => TransitionTo(Idle);

    private void Update() => CurrentState?.Update();

    // ---- 公開メソッド ----

    public void TransitionTo(EnemyState next)
    {
        CurrentState?.Exit();
        CurrentState = next;
        CurrentState.Enter();
    }

    /// <summary>怯み状態に遷移する。ダメージ処理から呼ぶ。</summary>
    public void TriggerStagger() => TransitionTo(Stagger);

    /// <summary>死亡状態に遷移する。CharacterStats.OnDeath から呼ぶ。</summary>
    public void TriggerDeath() => TransitionTo(Death);

    /// <summary>
    /// ノックバックを発動する。KnockbackHitEffect から呼ぶ。
    /// 死亡中はノックバックしない。
    /// </summary>
    public void TriggerKnockback(Vector3 dir, float distance, float duration)
    {
        if (CurrentState is EnemyDeathState) return;

        Knockback.SetKnockback(dir, distance, duration);
        TransitionTo(Knockback);
    }
}
