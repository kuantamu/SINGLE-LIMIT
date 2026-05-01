using UnityEngine;

/// <summary>
/// プレイヤーの状態機械。
/// 現段階では Idle / Guard / Dodge / Attack。
/// </summary>
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerAnimationController))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerStateMachine : MonoBehaviour
{
    // ---- 参照 ----
    public PlayerInputHandler        InputHandler   { get; private set; }
    public PlayerMovement            Movement       { get; private set; }
    public PlayerAnimationController AnimController { get; private set; }
    // ---- 設定 ----
    [Header("回避設定")]
    [SerializeField] public DodgeSettings DodgeConfig = new DodgeSettings();

    // ---- ステートインスタンス ----
    public IdleState   Idle   { get; private set; }
    public GuardState  Guard  { get; private set; }
    public DodgeState  Dodge  { get; private set; }
    public AttackState      Attack { get; private set; }
    public PlayerDeathState Death  { get; private set; }
    public SpecialState Special { get; private set; }
    public HeavyAttackState HeavyAttack { get; private set; }

    // ---- 現在のステート ----
    public PlayerState CurrentState { get; private set; }

    // ---- 回避クールダウン ----
    public float DodgeCooldownTimer { get; set; }
    public bool  IsDodgeOnCooldown  => DodgeCooldownTimer > 0f;

    // ---- Unity ライフサイクル ----

    private void Awake()
    {
        InputHandler   = GetComponent<PlayerInputHandler>();
        Movement       = GetComponent<PlayerMovement>();
        AnimController = GetComponent<PlayerAnimationController>();
        Idle   = new IdleState(this);
        Guard  = new GuardState(this);
        Dodge  = new DodgeState(this);
        Attack = new AttackState(this);
        Death  = new PlayerDeathState(this);
        Special = new SpecialState(this);
        HeavyAttack = new HeavyAttackState(this);
    }

    private void Start() => TransitionTo(Idle);

    private void Update()
    {
        if (DodgeCooldownTimer > 0f)
            DodgeCooldownTimer -= Time.deltaTime;

        CurrentState?.Update();
    }

    /// <summary>
    /// 死亡状態に遷移する。CharacterStats.OnDeath から呼ぶ。
    /// </summary>
    public void TriggerDeath() => TransitionTo(Death);

    public void TransitionTo(PlayerState next)
    {
        CurrentState?.Exit();
        CurrentState = next;
        CurrentState.Enter();
    }
}
