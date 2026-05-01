using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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

    [Header("回避")]
    [Tooltip("回避の移動距離（m）")]
    [SerializeField] private float _dodgeDistance = 4f;

    [Tooltip("回避の移動秒数（s）※ 前隙・後隙は含まない")]
    [SerializeField] private float _dodgeActiveDuration = 0.3f;

    private Rigidbody _rb;
    private Camera    _cam;
    private Vector3   _horizontalVel;

    private bool    _isDodging;
    private float _dodgeTimer;
    private Vector3 _dodgeVelocity;
    private bool _isFast = false;
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

    //毎フレーム呼ぶ。input がゼロなら減速して停止する。
    public void Move(Vector2 input)
    {
        Vector3 dir = CameraRelativeDirection(input);
        float MoveSpeed;
        if (_isFast)  {
            MoveSpeed = _fastSpeed;
        }
        else {
            MoveSpeed = _moveSpeed;
        }

        Vector3 target = dir * MoveSpeed;

        //float accel = input.magnitude > 0.1f ? _acceleration : _deceleration;
        //_horizontalVel = Vector3.MoveTowards(_horizontalVel, target, accel * Time.deltaTime);
        _horizontalVel = target;


        if (dir != Vector3.zero)
            FaceDirection(dir);
    }

    public void FastSpeed(bool f)
    {
        _isFast = f;
    }

    //回避の移動フェーズを開始する。DodgeState の Active フェーズ開始時に呼ぶ。
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

        // 向きは変えない
    }

    //回避の移動を停止する。DodgeState の Active フェーズ終了時に呼ぶ。
    public void StopDodge()
    {
        _isDodging     = false;
        _dodgeTimer    = 0f;
        _dodgeVelocity = Vector3.zero;
        StopHorizontal();
    }
    //カメラの前方向きにキャラクターを向ける。
    public void FaceCamera()
    {
        Vector3 camForward = _cam.transform.forward;
        camForward.y = 0f;
        if (camForward.sqrMagnitude < 0.001f) return;
        FaceDirection(camForward.normalized);
    }

    //水平速度をゼロにする。
    public void StopHorizontal()
    {
        _horizontalVel = Vector3.zero;
        Vector3 vel = _rb.linearVelocity;
        vel.x = 0f;
        vel.z = 0f;
        _rb.linearVelocity = vel;
    }

    private Vector3 CameraRelativeDirection(Vector2 input)
    {
        if (input.magnitude < 0.1f) return Vector3.zero;

        Vector3 forward = _cam.transform.forward;
        Vector3 right   = _cam.transform.right;
        forward = InputNomarized(forward);
        right = InputNomarized(right);

        return (forward * input.y + right * input.x).normalized;
    }

    private void FaceDirection(Vector3 dir)
    {
        if (dir == Vector3.zero) return;
        transform.rotation = Quaternion.LookRotation(dir);
    }
    private Vector3 InputNomarized(Vector3 input)
    {
        input.y = 0;
        return input.normalized;
    }
}
