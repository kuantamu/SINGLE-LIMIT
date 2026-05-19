using UnityEngine;

//キャラクターの基底移動スクリプト
[RequireComponent(typeof(Rigidbody))]
public abstract class CharacterMovementBase : MonoBehaviour
{
    protected Rigidbody Rb { get; private set; }
    protected Vector3 HorizontalVelocity;

    private bool _isKnockback;
    private float _knockbackTimer;
    private Vector3 _knockbackVelocity;

    protected virtual void Awake()
    {
        Rb = GetComponent<Rigidbody>();
        Rb.freezeRotation = true;
        Rb.useGravity = true;
    }

    protected virtual void FixedUpdate()
    {
        TickKnockback();
        ApplyHorizontalVelocity();
    }

    protected void TickKnockback()
    {
        if (!_isKnockback) return;

        _knockbackTimer -= Time.fixedDeltaTime;
        if (_knockbackTimer <= 0f)
            StopKnockback();
        else
            HorizontalVelocity = _knockbackVelocity;
    }

    protected void ApplyHorizontalVelocity()
    {

        Vector3 vel = Rb.linearVelocity;
        vel.x = HorizontalVelocity.x;
        vel.z = HorizontalVelocity.z;
        Rb.linearVelocity = vel;

    }

    public void StopHorizontal()
    {
        HorizontalVelocity = Vector3.zero;
        Vector3 vel = Rb.linearVelocity;
        vel.x = 0f;
        vel.z = 0f;
        Rb.linearVelocity = vel;
    }

    /// <summary>
    /// 攻撃アシストシステムから速度を上書きする。
    /// ノックバック中は効果がない（ノックバックが優先される）。
    /// </summary>
    public void SetAssistVelocity(Vector3 vel)
    {
        if (_isKnockback) return;
        HorizontalVelocity = vel;
    }

    public void StartKnockback(Vector3 dir, float distance, float duration)
    {
        if (duration <= 0f) return;

        float speed = distance / duration;
        _knockbackVelocity = dir * speed;
        HorizontalVelocity = _knockbackVelocity;
        _isKnockback = true;
        _knockbackTimer = duration;
    }

    public void StopKnockback()
    {
        _isKnockback = false;
        _knockbackTimer = 0f;
        _knockbackVelocity = Vector3.zero;
        StopHorizontal();
    }

    protected void FaceDirection(Vector3 dir, float rotationSpeed)
    {
        if (dir == Vector3.zero) return;
        if (rotationSpeed >= 3000)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }
        else
        {
            Quaternion target = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, target, rotationSpeed * Time.deltaTime);
        }
    }

    protected Vector3 FlattenAndNormalize(Vector3 v)
    {
        v.y = 0f;
        return v.normalized;
    }
}
