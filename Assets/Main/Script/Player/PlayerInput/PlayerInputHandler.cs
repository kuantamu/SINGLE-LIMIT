using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    #region 変数
    [Header("強攻撃の長押し閾値（秒）")]
    [SerializeField] private float _heavyAttackThreshold = 0.5f;

    /// <summary>WASD の入力ベクトル（正規化なし）</summary>
    public Vector2 MoveInput { get; private set; }

    /// <summary>移動入力があるか</summary>
    public bool IsMoving => MoveInput.magnitude > 0.1f;

    /// <summary>右クリックを押し続けているか（防御ホールド判定）</summary>
    public bool IsGuardHeld { get; private set; }

    /// <summary>左クリックを押し続けているか</summary>
    public bool IsAttackHeld { get; private set; }

    /// <summary>速く移動する</summary>
    public bool IsFastSpeedHeld { get; private set; }

    /// <summary>
    /// 通常攻撃が確定した瞬間か。
    /// 左クリックを離した時、長押し時間が閾値未満なら true。
    /// </summary>
    public bool AttackPressed { get; private set; }

    /// <summary>
    /// 強攻撃が確定した瞬間か。
    /// 左クリックを離した時、長押し時間が閾値以上なら true。
    /// </summary>
    public bool HeavyAttackPressed { get; private set; }

    /// <summary>
    /// 長押し進捗（0〜1）。
    /// UI やエフェクトのチャージ表示に使える。
    /// </summary>
    public float HeavyAttackCharge =>
        Mathf.Clamp01(_holdTimer / _heavyAttackThreshold);

    /// <summary>現在チャージ中か（閾値に達していない長押し状態）</summary>
    public bool IsCharging => IsAttackHeld && _holdTimer < _heavyAttackThreshold;

    /// <summary>チャージが完了しているか（長押し中かつ閾値到達）</summary>
    public bool IsChargeComplete => IsAttackHeld && _holdTimer >= _heavyAttackThreshold;

    /// <summary>特殊攻撃ボタンが押された瞬間か</summary>
    public bool SpecialPressed { get; private set; }

    /// <summary>バッファに攻撃入力が残っているか</summary>
    public bool HasBufferedAttack => _buffer.HasAttack;

    /// <summary>攻撃の入力を防ぐ</summary>
    public bool AttackLock;

    // ---- 内部 ----
    private readonly InputBuffer _buffer = new InputBuffer();
    private bool _bufferOpen;
    private float _holdTimer; // 左クリックを押し続けた時間
    #endregion
    private void Update()
    {
        float h   = Input.GetAxisRaw("Horizontal");
        float v   = Input.GetAxisRaw("Vertical");
        MoveInput = new Vector2(h, v);

        IsGuardHeld     = Input.GetMouseButton(1);
        IsAttackHeld    = Input.GetMouseButton(0);
        SpecialPressed = Input.GetKeyDown(KeyCode.Q);
        IsFastSpeedHeld = Input.GetKey(KeyCode.LeftShift);

        if (Input.GetMouseButtonDown(0)) _holdTimer = 0;
        if (IsAttackHeld) _holdTimer += Time.deltaTime;

        AttackPressed = false;
        HeavyAttackPressed = false;


        if (_holdTimer >= _heavyAttackThreshold && AttackLock == false)
            HeavyAttackPressed = true;

        if (Input.GetMouseButtonUp(0))
        {
            if(AttackLock == false)AttackPressed = true;
            _holdTimer = 0f;
            AttackLock = false;
        }




        // バッファが開いている時だけ積み込む
        if (AttackPressed && _bufferOpen)
            _buffer.SetAttack();

        _buffer.Tick(Time.deltaTime);
    }

    #region バッファ関連
    /// <summary>バッファへの積み込みを許可する。</summary>
    public void OpenBuffer()
    {
        _bufferOpen = true;
    }

    /// <summary>
    /// バッファを閉じてクリアする。
    /// AttackState.Exit() や他行動への遷移時に呼ぶ。
    /// </summary>
    public void CloseAndCancelBuffer()
    {
        _bufferOpen = false;
        _buffer.Cancel();
    }

    /// <summary>バッファの攻撃入力を消費する。</summary>
    public bool ConsumeBufferedAttack() => _buffer.ConsumeAttack();

    public void CancelBuffer() => CloseAndCancelBuffer();

    #endregion
}
