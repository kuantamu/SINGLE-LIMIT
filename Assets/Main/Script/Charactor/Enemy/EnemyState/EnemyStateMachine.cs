using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(EnemyAnimationController))]
[RequireComponent(typeof(EnemyDetector))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CharacterStats))]
public class EnemyStateMachine : CharacterStateMachineBase
{
    public EnemyMovement Movement { get; private set; }
    public EnemyAnimationController AnimController { get; private set; }
    public EnemyDetector Detector { get; private set; }
    public CharacterStats CharStats => Stats;

    [Header("Attack Range")]
    [SerializeField] private float _attackRange = 2f;

    [Header("Attack Interval")]
    [SerializeField] private float _attackInterval = 2f;

    public EnemyIdleState Idle { get; private set; }
    public EnemyChaseState Chase { get; private set; }
    public EnemyAttackState Attack { get; private set; }
    public EnemyStaggerState Stagger { get; private set; }
    public EnemyDeathState Death { get; private set; }
    public EnemyKnockbackState Knockback { get; private set; }

    public float AttackRange => _attackRange;
    public float AttackInterval => _attackInterval;
    public Transform PlayerTransform => Detector.DetectedPlayer;

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

    protected override void InitializeStateMachine()
    {
        Movement = GetComponent<EnemyMovement>();
        AnimController = GetComponent<EnemyAnimationController>();
        Detector = GetComponent<EnemyDetector>();

        Idle = new EnemyIdleState(this);
        Chase = new EnemyChaseState(this);
        Attack = new EnemyAttackState(this);
        Stagger = new EnemyStaggerState(this);
        Death = new EnemyDeathState(this);
        Knockback = new EnemyKnockbackState(this);
    }

    protected override ICharacterState GetInitialState() => Idle;
    protected override ICharacterState GetDeathState() => Death;

    public void TriggerStagger()
    {
        if (IsDeadState) return;
        if (CharStats != null && !CharStats.CanBeKnockedBack) return;
        TransitionTo(Stagger);
    }

    public override void TriggerKnockback(Vector3 dir, float distance, float duration)
    {
        if (IsDeadState) return;
        if (CharStats != null && !CharStats.CanBeKnockedBack) return;

        CharStats?.SetTimedHitReactionState(HitReactionState.Down, duration);
        Knockback.SetKnockback(dir, distance, duration);
        TransitionTo(Knockback);
    }
}
