using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerAnimationController))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CharacterStats))]
public class PlayerStateMachine : CharacterStateMachineBase
{
    public PlayerInputHandler InputHandler { get; private set; }
    public PlayerMovement Movement { get; private set; }
    public PlayerAnimationController AnimController { get; private set; }
    public CharacterStats CharStats => Stats;

    [Header("Dodge Settings")]
    [SerializeField] public DodgeSettings DodgeConfig = new DodgeSettings();

    public IdleState Idle { get; private set; }
    public GuardState Guard { get; private set; }
    public DodgeState Dodge { get; private set; }
    public AttackState Attack { get; private set; }
    public PlayerDeathState Death { get; private set; }
    public SpecialState Special { get; private set; }
    public HeavyAttackState HeavyAttack { get; private set; }
    public PlayerKnockbackState Knockback { get; private set; }

    public float DodgeCooldownTimer { get; set; }
    public bool IsDodgeOnCooldown => DodgeCooldownTimer > 0f;
    public int CurrentDodgePenaltyLevel => CalculateDodgePenaltyLevel();

    private float _dodgePenalty;
    private float _lastDodgeUseTime = float.NegativeInfinity;
    private float _nextDodgePenaltyDecayTime = float.PositiveInfinity;

    protected override void InitializeStateMachine()
    {
        InputHandler = GetComponent<PlayerInputHandler>();
        Movement = GetComponent<PlayerMovement>();
        AnimController = GetComponent<PlayerAnimationController>();

        Idle = new IdleState(this);
        Guard = new GuardState(this);
        Dodge = new DodgeState(this);
        Attack = new AttackState(this);
        Death = new PlayerDeathState(this);
        Special = new SpecialState(this);
        HeavyAttack = new HeavyAttackState(this);
        Knockback = new PlayerKnockbackState(this);
    }

    protected override ICharacterState GetInitialState() => Idle;
    protected override ICharacterState GetDeathState() => Death;

    protected override void OnBeforeStateUpdate()
    {
        if (DodgeCooldownTimer > 0f)
            DodgeCooldownTimer -= Time.deltaTime;

        TickDodgePenalty();
    }

    /// <summary>回避を使った瞬間にペナルティを加算し、今回の段階を返す。</summary>
    public int RegisterDodgeUse()
    {
        bool isRepeatedDodge = Time.time - _lastDodgeUseTime <= DodgeConfig.RepeatWindow;
        _dodgePenalty += DodgeConfig.BasePenalty;
        if (isRepeatedDodge)
            _dodgePenalty += DodgeConfig.RepeatPenaltyBonus;
        _dodgePenalty = Mathf.Min(
            _dodgePenalty,
            Mathf.Max(0, DodgeConfig.MaxPenaltyLevel) * Mathf.Max(0.001f, DodgeConfig.PenaltyPerLevel));

        _lastDodgeUseTime = Time.time;
        _nextDodgePenaltyDecayTime = Time.time + DodgeConfig.PenaltyDecayDelay;
        return CalculateDodgePenaltyLevel();
    }

    public float GetDodgeLagMultiplier(int penaltyLevel)
    {
        return 1f + Mathf.Max(0, penaltyLevel) * Mathf.Max(0f, DodgeConfig.LagPenaltyPerLevel);
    }

    public float GetDodgeInvincibleDuration(int penaltyLevel)
    {
        float multiplier = 1f - Mathf.Max(0, penaltyLevel) * Mathf.Max(0f, DodgeConfig.InvinciblePenaltyPerLevel);
        multiplier = Mathf.Max(DodgeConfig.MinInvincibleMultiplier, multiplier);
        return DodgeConfig.InvincibleDuration * multiplier;
    }

    private void TickDodgePenalty()
    {
        if (_dodgePenalty <= 0f || Time.time < _nextDodgePenaltyDecayTime) return;

        int level = CalculateDodgePenaltyLevel();
        if (level <= 0)
        {
            _dodgePenalty = 0f;
            _nextDodgePenaltyDecayTime = float.PositiveInfinity;
            return;
        }

        // 段階が下がるタイミングで小数点以下を消し、次の段階の値へ丸める。
        _dodgePenalty = Mathf.Max(0, level - 1) * Mathf.Max(0.001f, DodgeConfig.PenaltyPerLevel);
        _nextDodgePenaltyDecayTime = Time.time + DodgeConfig.PenaltyDecayInterval;
    }

    private int CalculateDodgePenaltyLevel()
    {
        float penaltyPerLevel = Mathf.Max(0.001f, DodgeConfig.PenaltyPerLevel);
        int level = Mathf.FloorToInt(_dodgePenalty / penaltyPerLevel);
        return Mathf.Clamp(level, 0, Mathf.Max(0, DodgeConfig.MaxPenaltyLevel));
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
