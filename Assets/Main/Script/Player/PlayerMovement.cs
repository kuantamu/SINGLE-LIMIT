using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    #region 変数関連
    [Header("移動")]
    [SerializeField] private float _moveSpeed    = 5f;
    [SerializeField] private float _acceleration = 20f;
    [SerializeField] private float _deceleration = 30f;

    [Header("加速")]
    [SerializeField] private float _fastSpeed = 15f;

    [Header("回転")]
    [Tooltip("移動方向への回転速度（deg/s）")]
    [SerializeField] private float _rotationSpeed = 720f;

    [Header("回避")]
    [Tooltip("回避の移動距離（m）")]
    [SerializeField] private float _dodgeDistance = 4f;

    [Tooltip("回避の移動秒数（s）※ 前隙・後隙は含まない")]
    [SerializeField] private float _dodgeActiveDuration = 0.3f;

    private Rigidbody _rb;
    private Camera    _cam;
    private Vector3   _horizontalVel;

    private bool    _isDodging;
    private float   _dodgeTimer;
    private Vector3 _dodgeVelocity;
    private bool    _isFast = false;

    private bool _isKnockback;
    private float _knockbackTimer;
    private Vector3 _knockbackVelocity;
    #endregion

    private void Awake()
    {
        _rb  = GetComponent<Rigidbody>();
        _cam = Camera.main;

        _rb.freezeRotation = true;
        _rb.useGravity     = true;
    }

    private void FixedUpdate()
    {
        if (_isKnockback)
        {
            _knockbackTimer -= Time.fixedDeltaTime;
            if (_knockbackTimer <= 0f)
                StopKnockback();
            else
                _horizontalVel = _knockbackVelocity;
        }
        if (_isDodging)
        {
            _dodgeTimer -= Time.fixedDeltaTime;
            if (_dodgeTimer <= 0f)
                StopDodge();
            else
                _horizontalVel = _dodgeVelocity;
        }

        Vector3 vel = _rb.linearVelocity;
        vel.x = _horizontalVel.x;
        vel.z = _horizontalVel.z;
        _rb.linearVelocity = vel;
    }

    /// <summary>
    /// 毎フレーム呼ぶ。input がゼロなら減速して停止する。
    /// </summary>
    public void Move(Vector2 input)
    {
        Vector3 dir   = CameraRelativeDirection(input);
        float speed   = _isFast ? _fastSpeed : _moveSpeed;
        Vector3 target = dir * speed;

        // 入力がある時は加速、ない時は減速
        float accel = input.magnitude > 0.1f ? _acceleration : _deceleration;
        _horizontalVel = Vector3.MoveTowards(
            _horizontalVel, target, accel * Time.deltaTime);

        if (dir != Vector3.zero)
            FaceDirection(dir);
    }

    public void FastSpeed(bool f)
    {
        _isFast = f;
    }

    /// <summary>
    /// 回避の移動フェーズを開始する。DodgeState の Active フェーズ開始時に呼ぶ。
    /// </summary>
    public void StartDodgeMove(Vector2 input)
    {
        Vector3 dir = CameraRelativeDirection(input);
        if (dir == Vector3.zero)
            dir = transform.forward;

        float speed    = _dodgeDistance / _dodgeActiveDuration;
        _dodgeVelocity = dir * speed;
        _horizontalVel = _dodgeVelocity;

        _isDodging  = true;
        _dodgeTimer = _dodgeActiveDuration;
    }

    /// <summary>
    /// 回避の移動を停止する。DodgeState の Active フェーズ終了時に呼ぶ。
    /// </summary>
    public void StopDodge()
    {
        _isDodging     = false;
        _dodgeTimer    = 0f;
        _dodgeVelocity = Vector3.zero;
        StopHorizontal();
    }

    /// <summary>
    /// カメラの前方向きにキャラクターを向ける。
    /// </summary>
    public void FaceCamera()
    {
        Vector3 camForward = _cam.transform.forward;
        camForward.y = 0f;
        if (camForward.sqrMagnitude < 0.001f) return;
        FaceDirection(camForward.normalized);
    }

    /// <summary>
    /// 水平速度をゼロにする。
    /// </summary>
    public void StopHorizontal()
    {
        _horizontalVel = Vector3.zero;
        Vector3 vel = _rb.linearVelocity;
        vel.x = 0f;
        vel.z = 0f;
        _rb.linearVelocity = vel;
    }

    // ---- 内部 ----

    private Vector3 CameraRelativeDirection(Vector2 input)
    {
        if (input.magnitude < 0.1f) return Vector3.zero;

        Vector3 forward = FlattenAndNormalize(_cam.transform.forward);
        Vector3 right   = FlattenAndNormalize(_cam.transform.right);

        return (forward * input.y + right * input.x).normalized;
    }

    /// <summary>
    /// 指定した方向へ滑らかに回転する。
    /// </summary>
    private void FaceDirection(Vector3 dir)
    {
        if (dir == Vector3.zero) return;
        Quaternion target = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, target, _rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// ベクトルの Y を 0 にして正規化する。
    /// </summary>
    private Vector3 FlattenAndNormalize(Vector3 v)
    {
        v.y = 0f;
        return v.normalized;
    }

    /// <summary>
    /// ノックバックを開始する。EnemyKnockbackState.Enter() から呼ぶ。
    /// </summary>
    /// <param name="dir">ノックバック方向（正規化済み）</param>
    /// <param name="distance">移動距離（m）</param>
    /// <param name="duration">移動時間（秒）</param>
    public void StartKnockback(Vector3 dir, float distance, float duration)
    {
        if (duration <= 0f) return;

        float speed = distance / duration;
        _knockbackVelocity = dir * speed;
        _horizontalVel = _knockbackVelocity;
        _isKnockback = true;
        _knockbackTimer = duration;
    }

    /// <summary>ノックバックを停止する。EnemyKnockbackState.Exit() から呼ぶ。</summary>
    public void StopKnockback()
    {
        _isKnockback = false;
        _knockbackTimer = 0f;
        _knockbackVelocity = Vector3.zero;
        StopHorizontal();
    }
}
