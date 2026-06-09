using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("強攻撃の長押し閾値（秒）")]
    [SerializeField] private float _heavyAttackThreshold = 0.5f;

    public Vector2 MoveInput { get; private set; }

    public bool IsMoving => MoveInput.magnitude > 0.1f;

    public bool IsGuardHeld { get; private set; }

    public bool IsAttackHeld { get; private set; }

    public bool IsFastSpeedHeld { get; private set; }

    public bool IsDodgePush { get; private set; }

    public bool AttackPressed { get; private set; }

    public bool HeavyAttackPressed { get; private set; }

    public float HeavyAttackCharge =>
        Mathf.Clamp01(_holdTimer / _heavyAttackThreshold);

    public bool IsCharging => IsAttackHeld && _holdTimer < _heavyAttackThreshold;

    public bool IsChargeComplete => IsAttackHeld && _holdTimer >= _heavyAttackThreshold;

    public bool SpecialPressed { get; private set; }

    public bool HasBufferedAttack => _buffer.HasAttack;

    public bool AttackLock { get; set; }

    private readonly InputBuffer _buffer = new InputBuffer();
    private bool  _bufferOpen;
    private float _holdTimer;        // 左クリックを押し続けた時間
    private bool  _heavyAttackFired; // 閾値到達後の多重発火を防ぐフラグ

    private void Update()
    {
        float h   = Input.GetAxisRaw("Horizontal");
        float v   = Input.GetAxisRaw("Vertical");
        MoveInput = new Vector2(h, v);

        IsGuardHeld     = Input.GetMouseButton(1);
        IsAttackHeld    = Input.GetMouseButton(0);
        SpecialPressed  = Input.GetKeyDown(KeyCode.Q);
        IsFastSpeedHeld = Input.GetKey(KeyCode.LeftShift);
        IsDodgePush     = Input.GetKeyDown(KeyCode.LeftShift);

        if (Input.GetMouseButtonDown(0))
        {
            _holdTimer        = 0f;
            _heavyAttackFired = false;
        }

        if (IsAttackHeld)
            _holdTimer += Time.deltaTime;

        AttackPressed      = false;
        HeavyAttackPressed = false;

        if (IsAttackHeld
            && _holdTimer >= _heavyAttackThreshold
            && !_heavyAttackFired
            && !AttackLock)
        {
            HeavyAttackPressed = true;
            _heavyAttackFired  = true; // 以降は発火しない
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (!AttackLock && !_heavyAttackFired)
                AttackPressed = true;

            _holdTimer = 0f;
            AttackLock = false;
            _heavyAttackFired = false;
        }

        if (AttackPressed && _bufferOpen)
            _buffer.SetAttack();

        _buffer.Tick(Time.deltaTime);
    }

    public void OpenBuffer()
    {
        _bufferOpen = true;
    }

    public void CloseAndCancelBuffer()
    {
        _bufferOpen = false;
        _buffer.Cancel();
    }

    public bool ConsumeBufferedAttack() => _buffer.ConsumeAttack();

    public void CancelBuffer() => CloseAndCancelBuffer();
}
