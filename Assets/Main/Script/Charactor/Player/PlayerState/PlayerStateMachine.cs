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
    }

    public override void TriggerKnockback(Vector3 dir, float distance, float duration)
    {
        if (IsDeadState) return;

        Knockback.SetKnockback(dir, distance, duration);
        TransitionTo(Knockback);
    }
}
