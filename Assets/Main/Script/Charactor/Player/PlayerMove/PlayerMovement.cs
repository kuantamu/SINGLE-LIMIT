using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : CharacterMovementBase
{
    #region　変数
    [Header("Move")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _acceleration = 20f;
    [SerializeField] private float _deceleration = 30f;

    [Header("Fast Move")]
    [SerializeField] private float _fastSpeed = 15f;

    [Header("Rotation")]
    [SerializeField] private float _rotationSpeed = 720f;

    [Header("Dodge")]
    [SerializeField] private float _dodgeDistance = 4f;
    [SerializeField] private float _dodgeActiveDuration = 0.3f;

    private Camera _cam;
    private ThirdPersonCamera _thirdPersonCamera;
    private bool _isDodging;
    private float _dodgeTimer;
    private Vector3 _dodgeVelocity;
    private bool _isFast;
    #endregion
    protected override void Awake()
    {
        base.Awake();
        _cam = Camera.main;
        _thirdPersonCamera = Camera.main.GetComponent<ThirdPersonCamera>();
    }

    protected override void FixedUpdate()
    {
        TickKnockback();

        if (_isDodging)
        {
            _dodgeTimer -= Time.fixedDeltaTime;
            if (_dodgeTimer <= 0f)
                StopDodge();
            else
                HorizontalVelocity = _dodgeVelocity;
        }

        ApplyHorizontalVelocity();
    }

    public void Move(Vector2 input)
    {
        Vector3 dir = CameraRelativeDirection(input);
        float speed = _isFast ? _fastSpeed : _moveSpeed;
        Vector3 target = dir * speed;

        float accel = input.magnitude > 0.1f ? _acceleration : _deceleration;
        HorizontalVelocity = Vector3.MoveTowards(
            HorizontalVelocity, target, accel * Time.deltaTime);

        if (dir != Vector3.zero)
            FaceDirection(dir, _rotationSpeed);
    }

    public void FastSpeed(bool f)
    {
        _isFast = f;
    }

    public void StartDodgeMove(Vector2 input)
    {
        Vector3 dir = CameraRelativeDirection(input);
        if (dir == Vector3.zero)
            dir = transform.forward;

        float speed = _dodgeDistance / _dodgeActiveDuration;
        _dodgeVelocity = dir * speed;
        HorizontalVelocity = _dodgeVelocity;

        _isDodging = true;
        _dodgeTimer = _dodgeActiveDuration;
    }

    public void StopDodge()
    {
        _isDodging = false;
        _dodgeTimer = 0f;
        _dodgeVelocity = Vector3.zero;
        StopHorizontal();
    }

    public void FaceCamera()
    {
        if (_cam == null) return;

        Vector3 camForward = _cam.transform.forward;
        camForward.y = 0f;
        if (camForward.sqrMagnitude < 0.001f) return;

        FaceDirection(camForward.normalized, _rotationSpeed);
    }

    public void FaceTarget()
    {
        Transform target = _thirdPersonCamera.lockOnTarget();
        if (target == null) return;

        Vector3 forward = target.position - transform.position;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f) return;

        FaceDirection(forward.normalized, 10000);
    }

    private Vector3 CameraRelativeDirection(Vector2 input)
    {
        if (_cam == null || input.magnitude < 0.1f) return Vector3.zero;

        Vector3 forward = FlattenAndNormalize(_cam.transform.forward);
        Vector3 right = FlattenAndNormalize(_cam.transform.right);

        return (forward * input.y + right * input.x).normalized;
    }
}
