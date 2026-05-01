using UnityEngine;


public class ThirdPersonCamera : MonoBehaviour
{
    // ---- Inspector ----

    [Header("追従対象")]
    [SerializeField] private Transform _target;

    [Tooltip("注視点のオフセット（キャラの腰〜頭の高さに合わせる）")]
    [SerializeField] private Vector3 _targetOffset = new Vector3(0f, 1.4f, 0f);

    [Header("距離")]
    [SerializeField] private float _distance    = 5f;
    [SerializeField] private float _minDistance = 1.5f;
    [SerializeField] private float _maxDistance = 10f;
    [SerializeField] private float _zoomSpeed   = 4f;

    [Header("回転感度")]
    [SerializeField] private float _horizontalSensitivity = 180f;
    [SerializeField] private float _verticalSensitivity   = 120f;

    [Header("垂直角度制限（度）")]
    [SerializeField] private float _verticalMin = -20f;
    [SerializeField] private float _verticalMax =  60f;

    [Header("スムージング")]
    [Tooltip("追従の滑らかさ（大きいほどキビキビ追う）")]
    [SerializeField] private float _followSpeed = 15f;

    [Tooltip("壁に当たった時に手前に引く速度")]
    [SerializeField] private float _collisionPullSpeed = 20f;

    [Header("壁抜け防止")]
    [SerializeField] private LayerMask _collisionMask;

    [Tooltip("SphereCast の半径（小さいほど壁に近づける）")]
    [SerializeField] private float _collisionRadius = 0.2f;

    // ---- 内部 ----

    private float _yaw;             // 水平角度
    private float _pitch;           // 垂直角度
    private float _currentDistance; // 実際の距離（壁抜け防止で縮む）
    private float _targetDistance;  // ズーム目標距離

    // ---- Unity ライフサイクル ----

    private void Awake()
    {
        if (_target == null)
            Debug.LogWarning("[ThirdPersonCamera] Target が未設定です。Inspector でプレイヤーを設定してください。");

        // 初期角度をカメラの現在の向きから取得
        _yaw   = transform.eulerAngles.y;
        _pitch = transform.eulerAngles.x;

        _currentDistance = _distance;
        _targetDistance  = _distance;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void FixedUpdate()
    {
        if (_target == null) return;

        HandleRotation();
        HandleZoom();
        ApplyTransform();
    }

    // ---- 内部処理 ----

    private void HandleRotation()
    {
        _yaw   += Input.GetAxis("Mouse X") * _horizontalSensitivity * Time.deltaTime;
        _pitch -= Input.GetAxis("Mouse Y") * _verticalSensitivity   * Time.deltaTime;
        _pitch  = Mathf.Clamp(_pitch, _verticalMin, _verticalMax);
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            _targetDistance -= scroll * _zoomSpeed;
            _targetDistance  = Mathf.Clamp(_targetDistance, _minDistance, _maxDistance);
        }

        // ズームをスムーズに適用
        _currentDistance = Mathf.Lerp(
            _currentDistance, _targetDistance, Time.deltaTime * _zoomSpeed);
    }

    private void ApplyTransform()
    {
        // 注視点
        Vector3    pivot    = _target.position + _targetOffset;
        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);

        // 壁抜け防止: 注視点からカメラ方向に SphereCast
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

        // 最終位置・回転を適用
        Vector3 finalPos = pivot + rotation * new Vector3(0f, 0f, -adjustedDistance);

        transform.position = Vector3.Lerp(
            transform.position, finalPos, Time.deltaTime * _followSpeed);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, rotation, Time.deltaTime * _followSpeed);
    }
}
