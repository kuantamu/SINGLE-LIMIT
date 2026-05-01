using UnityEngine;

/// <summary>
/// 敵の移動を管理する。Rigidbody ベース。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class EnemyMovement : MonoBehaviour
{
    [Header("移動")]
    [SerializeField] private float _moveSpeed    = 3f;
    [SerializeField] private float _acceleration = 15f;
    [SerializeField] private float _deceleration = 20f;

    [Tooltip("プレイヤーへの回転速度（deg/s）")]
    [SerializeField] private float _rotationSpeed = 360f;

    private Rigidbody _rb;
    private Vector3   _horizontalVel;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
        _rb.useGravity     = true;
    }

    // ---- ノックバック ----
    private bool    _isKnockback;
    private float   _knockbackTimer;
    private Vector3 _knockbackVelocity;

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

        Vector3 vel = _rb.linearVelocity;
        vel.x = _horizontalVel.x;
        vel.z = _horizontalVel.z;
        _rb.linearVelocity = vel;
    }

    /// <summary>
    /// 目標位置へ向かって移動する。ChaseState の毎フレームで呼ぶ。
    /// </summary>
    public void MoveToward(Vector3 targetPos)
    {
        Vector3 dir    = (targetPos - transform.position);
        dir.y          = 0f;
        dir.Normalize();

        Vector3 target = dir * _moveSpeed;
        _horizontalVel = Vector3.MoveTowards(
            _horizontalVel, target, _acceleration * Time.deltaTime);

        RotateToward(targetPos);
    }

    /// <summary>
    /// 目標方向へ向くだけで移動しない。AttackState などで呼ぶ。
    /// </summary>
    public void FaceToward(Vector3 targetPos)
    {
        RotateToward(targetPos);
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

        float speed       = distance / duration;
        _knockbackVelocity = dir * speed;
        _horizontalVel    = _knockbackVelocity;
        _isKnockback      = true;
        _knockbackTimer   = duration;
    }

    /// <summary>ノックバックを停止する。EnemyKnockbackState.Exit() から呼ぶ。</summary>
    public void StopKnockback()
    {
        _isKnockback      = false;
        _knockbackTimer   = 0f;
        _knockbackVelocity = Vector3.zero;
        StopHorizontal();
    }

    /// <summary>水平速度をゼロにする。Idle・Attack などの Enter() で呼ぶ。</summary>
    public void StopHorizontal()
    {
        _horizontalVel = Vector3.zero;
        Vector3 vel = _rb.linearVelocity;
        vel.x = 0f;
        vel.z = 0f;
        _rb.linearVelocity = vel;
    }

    // ---- 内部 ----

    private void RotateToward(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion target = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, target, _rotationSpeed * Time.deltaTime);
    }
}
