using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _targetOffset = new Vector3(0f, 1.4f, 0f);

    [Header("Distance")]
    [SerializeField] private float _distance = 5f;
    [SerializeField] private float _minDistance = 1.5f;
    [SerializeField] private float _maxDistance = 10f;
    [SerializeField] private float _zoomSpeed = 4f;

    [Header("Sensitivity")]
    [SerializeField] private float _horizontalSensitivity = 180f;
    [SerializeField] private float _verticalSensitivity = 120f;

    [Header("Vertical Limit")]
    [SerializeField] private float _verticalMin = -20f;
    [SerializeField] private float _verticalMax = 60f;

    [Header("Smoothing")]
    [SerializeField] private float _followSpeed = 15f;
    [SerializeField] private float _collisionPullSpeed = 20f;

    [Header("Collision")]
    [SerializeField] private LayerMask _collisionMask;
    [SerializeField] private float _collisionRadius = 0.2f;

    [Header("Lock On")]
    [SerializeField] private float _lockOnRotationSpeed = 12f;
    [SerializeField] private float _resetPitch = 0f;

    private float _yaw;
    private float _pitch;
    private float _currentDistance;
    private float _targetDistance;
    private Transform _lockOnTarget;

    private void Awake()
    {
        if (_target == null)
            Debug.LogWarning("[ThirdPersonCamera] Target is not assigned. Set the player in the Inspector.");

        _yaw = transform.eulerAngles.y;
        _pitch = transform.eulerAngles.x;

        _currentDistance = _distance;
        _targetDistance = _distance;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (_target == null) return;

        if (_lockOnTarget != null)
            HandleLockOnRotation();
        else
            HandleRotationInput();

        HandleZoomInput();
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        SmoothZoom();
        ApplyTransform();
    }

    public void SetLockOnTarget(Transform target)
    {
        _lockOnTarget = target;
    }

    public void ClearLockOn(bool resetCamera)
    {
        _lockOnTarget = null;

        if (resetCamera)
            ResetLookForward();
    }

    private void HandleRotationInput()
    {
        _yaw += Input.GetAxis("Mouse X") * _horizontalSensitivity * Time.deltaTime;
        _pitch -= Input.GetAxis("Mouse Y") * _verticalSensitivity * Time.deltaTime;
        _pitch = Mathf.Clamp(_pitch, _verticalMin, _verticalMax);
    }

    private void HandleLockOnRotation()
    {
        if (_lockOnTarget == null) return;

        Vector3 pivot = _target.position + _targetOffset;
        Vector3 toTarget = LockOnTargetUtility.GetAimPoint(_lockOnTarget) - pivot;
        if (toTarget.sqrMagnitude < 0.001f) return;

        Vector3 flat = toTarget;
        flat.y = 0f;
        if (flat.sqrMagnitude < 0.001f) return;

        float desiredYaw = Mathf.Atan2(flat.x, flat.z) * Mathf.Rad2Deg;
        float desiredPitch = -Mathf.Atan2(toTarget.y, flat.magnitude) * Mathf.Rad2Deg;
        desiredPitch = Mathf.Clamp(desiredPitch, _verticalMin, _verticalMax);

        _yaw = Mathf.LerpAngle(_yaw, desiredYaw, Time.deltaTime * _lockOnRotationSpeed);
        _pitch = Mathf.Lerp(_pitch, desiredPitch, Time.deltaTime * _lockOnRotationSpeed);
    }

    private void HandleZoomInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) <= 0.001f) return;

        _targetDistance -= scroll * _zoomSpeed;
        _targetDistance = Mathf.Clamp(_targetDistance, _minDistance, _maxDistance);
    }

    private void SmoothZoom()
    {
        _currentDistance = Mathf.Lerp(
            _currentDistance,
            _targetDistance,
            Time.deltaTime * _zoomSpeed);
    }

    private void ResetLookForward()
    {
        if (_target == null) return;

        _yaw = _target.eulerAngles.y;
        _pitch = Mathf.Clamp(_resetPitch, _verticalMin, _verticalMax);
    }

    private void ApplyTransform()
    {
        Vector3 pivot = _target.position + _targetOffset;
        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);

        float adjustedDistance = _currentDistance;
        Vector3 castDir = rotation * Vector3.back;

        if (Physics.SphereCast(
                pivot,
                _collisionRadius,
                castDir,
                out RaycastHit hit,
                _currentDistance,
                _collisionMask))
        {
            adjustedDistance = Mathf.Lerp(
                adjustedDistance,
                hit.distance,
                Time.deltaTime * _collisionPullSpeed);
        }

        Vector3 finalPos = pivot + rotation * new Vector3(0f, 0f, -adjustedDistance);

        transform.position = Vector3.Lerp(
            transform.position,
            finalPos,
            Time.deltaTime * _followSpeed);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rotation,
            Time.deltaTime * _followSpeed);
    }
}
