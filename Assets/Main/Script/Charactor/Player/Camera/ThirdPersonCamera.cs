using UnityEngine;

/// <summary>
/// 三人称視点カメラの制御クラス。
/// 通常時はマウス入力で自由回転、ロックオン中はターゲットへ自動追従する。
/// コリジョン回避、ズーム、カメラスムージングも担当する。
/// </summary>
public class ThirdPersonCamera : MonoBehaviour
{
    // --- インスペクター設定 ---

    [Header("Target")]
    /// <summary>カメラが追従するプレイヤーの Transform</summary>
    [SerializeField] private Transform _target;

    /// <summary>カメラが注目するピボット点のオフセット（腰〜頭の高さに調整する）</summary>
    [SerializeField] private Vector3 _targetOffset = new Vector3(0f, 1.4f, 0f);

    [Header("Distance")]
    /// <summary>プレイヤーからカメラまでの初期距離（メートル）</summary>
    [SerializeField] private float _distance = 5f;

    /// <summary>ズームインの最小距離（これより近くはズームできない）</summary>
    [SerializeField] private float _minDistance = 1.5f;

    /// <summary>ズームアウトの最大距離（これより遠くはズームできない）</summary>
    [SerializeField] private float _maxDistance = 10f;

    /// <summary>スクロールホイールによるズーム速度</summary>
    [SerializeField] private float _zoomSpeed = 4f;

    [Header("Sensitivity")]
    /// <summary>マウスの水平方向（左右）感度（度/秒）</summary>
    [SerializeField] private float _horizontalSensitivity = 180f;

    /// <summary>マウスの垂直方向（上下）感度（度/秒）</summary>
    [SerializeField] private float _verticalSensitivity = 120f;

    [Header("Vertical Limit")]
    /// <summary>カメラが下を向ける最大角度（負値 = 見下ろし）</summary>
    [SerializeField] private float _verticalMin = -20f;

    /// <summary>カメラが上を向ける最大角度（正値 = 見上げ）</summary>
    [SerializeField] private float _verticalMax = 60f;

    [Header("Smoothing")]
    /// <summary>カメラ位置追従のスムージング速度（大きいほど追従が速い）</summary>
    [SerializeField] private float _followSpeed = 15f;

    /// <summary>コリジョン回避時にカメラを引き寄せる速度</summary>
    [SerializeField] private float _collisionPullSpeed = 20f;

    [Header("Collision")]
    /// <summary>カメラコリジョン判定を行うレイヤーマスク（壁・床など）</summary>
    [SerializeField] private LayerMask _collisionMask;

    /// <summary>SphereCast に使うカメラコリジョン球の半径</summary>
    [SerializeField] private float _collisionRadius = 0.2f;

    [Header("Lock On")]
    /// <summary>ロックオン中にカメラが目標方向へ回転する速度</summary>
    [SerializeField] private float _lockOnRotationSpeed = 12f;

    /// <summary>ロックオン解除時にカメラをリセットする垂直角度（Pitch）</summary>
    [SerializeField] private float _resetPitch = 0f;

    // --- プライベートフィールド ---

    /// <summary>現在の水平回転角度（Y 軸, 世界基準）</summary>
    private float _yaw;

    /// <summary>現在の垂直回転角度（X 軸, 上下）</summary>
    private float _pitch;

    /// <summary>スムージング後の現在のカメラ距離</summary>
    private float _currentDistance;

    /// <summary>スクロール入力により設定された目標距離</summary>
    private float _targetDistance;

    /// <summary>ロックオン中のターゲット Transform（null = 通常操作）</summary>
    private Transform _lockOnTarget;

    /// <summary>ロックオン中のターゲット を返す</summary>
    public Transform lockOnTarget() {  return _lockOnTarget; }

    // --- Unity ライフサイクル ---

    private void Awake()
    {
        if (_target == null)
            Debug.LogWarning("[ThirdPersonCamera] Target is not assigned. Set the player in the Inspector.");

        // 初期の Yaw/Pitch を現在のカメラ向きから取得
        _yaw = transform.eulerAngles.y;
        _pitch = transform.eulerAngles.x;

        _currentDistance = _distance;
        _targetDistance = _distance;

        // マウスカーソルをゲーム中は非表示・ロックする
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (_target == null) return;

        // ロックオン中は自動回転、通常時はマウス入力で回転
        if (_lockOnTarget != null)
            HandleLockOnRotation();
        else
            HandleRotationInput();

        HandleZoomInput();
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        // Update() で計算した値をもとに、物理演算後に位置・回転を確定させる
        SmoothZoom();
        ApplyTransform();
    }

    // --- パブリックメソッド ---

    /// <summary>
    /// ロックオン対象を設定する。以降 HandleLockOnRotation がカメラを自動追従させる。
    /// LockOnController から呼ばれる。
    /// </summary>
    /// <param name="target">追従先の Transform</param>
    public void SetLockOnTarget(Transform target)
    {
        _lockOnTarget = target;
    }

    /// <summary>
    /// ロックオンを解除する。
    /// </summary>
    /// <param name="resetCamera">true のとき、カメラをプレイヤーの正面方向にリセットする</param>
    public void ClearLockOn(bool resetCamera)
    {
        _lockOnTarget = null;

        if (resetCamera)
            ResetLookForward();
    }

    // --- プライベートメソッド ---

    /// <summary>
    /// マウス入力から Yaw/Pitch を更新する（通常操作時）。
    /// Pitch は上下限でクランプされる。
    /// </summary>
    private void HandleRotationInput()
    {
        _yaw += Input.GetAxis("Mouse X") * _horizontalSensitivity * Time.deltaTime;
        _pitch -= Input.GetAxis("Mouse Y") * _verticalSensitivity * Time.deltaTime;
        _pitch = Mathf.Clamp(_pitch, _verticalMin, _verticalMax);
    }

    /// <summary>
    /// ロックオン中、カメラがターゲット方向を向くよう Yaw/Pitch を補間する。
    /// ピボット（プレイヤーのオフセット位置）からターゲットへのベクトルを基準に計算する。
    /// </summary>
    private void HandleLockOnRotation()
    {
        if (_lockOnTarget == null) return;

        Vector3 pivot = _target.position + _targetOffset;
        Vector3 toTarget = LockOnTargetUtility.GetAimPoint(_lockOnTarget) - pivot;
        if (toTarget.sqrMagnitude < 0.001f) return;

        // 水平成分のみで Yaw を求める
        Vector3 flat = toTarget;
        flat.y = 0f;
        if (flat.sqrMagnitude < 0.001f) return;

        float desiredYaw = Mathf.Atan2(flat.x, flat.z) * Mathf.Rad2Deg;

        // 垂直角度（Pitch）を求め、上下限でクランプ
        float desiredPitch = -Mathf.Atan2(toTarget.y, flat.magnitude) * Mathf.Rad2Deg;
        desiredPitch = Mathf.Clamp(desiredPitch, _verticalMin, _verticalMax);

        // 現在値から目標値へ滑らかに補間
        _yaw = Mathf.LerpAngle(_yaw, desiredYaw, Time.deltaTime * _lockOnRotationSpeed);
        _pitch = Mathf.Lerp(_pitch, desiredPitch, Time.deltaTime * _lockOnRotationSpeed);
    }

    /// <summary>
    /// マウスホイール入力から目標距離を更新する。
    /// _minDistance 〜 _maxDistance にクランプされる。
    /// </summary>
    private void HandleZoomInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) <= 0.001f) return;

        _targetDistance -= scroll * _zoomSpeed;
        _targetDistance = Mathf.Clamp(_targetDistance, _minDistance, _maxDistance);
    }

    /// <summary>
    /// _currentDistance を _targetDistance へ向けて Lerp で滑らかに補間する。
    /// </summary>
    private void SmoothZoom()
    {
        _currentDistance = Mathf.Lerp(
            _currentDistance,
            _targetDistance,
            Time.deltaTime * _zoomSpeed);
    }

    /// <summary>
    /// ロックオン解除時にカメラをプレイヤーの向きに合わせてリセットする。
    /// Pitch は _resetPitch（デフォルト 0 = 水平）にリセットする。
    /// </summary>
    private void ResetLookForward()
    {
        if (_target == null) return;

        _yaw = _target.eulerAngles.y;
        _pitch = Mathf.Clamp(_resetPitch, _verticalMin, _verticalMax);
    }

    /// <summary>
    /// Yaw/Pitch/Distance からカメラの最終位置と回転を計算し、Transform に適用する。
    /// SphereCast でコリジョンを検出し、壁に埋まらないよう距離を補正する。
    /// </summary>
    private void ApplyTransform()
    {
        Vector3 pivot = _target.position + _targetOffset;
        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);

        float adjustedDistance = _currentDistance;

        // ピボットからカメラ方向へ SphereCast を飛ばして壁との衝突を検出
        Vector3 castDir = rotation * Vector3.back;
        if (Physics.SphereCast(
                pivot,
                _collisionRadius,
                castDir,
                out RaycastHit hit,
                _currentDistance,
                _collisionMask))
        {
            // 衝突点まで距離を縮める（急激な変化を Lerp で緩和）
            adjustedDistance = Mathf.Lerp(
                adjustedDistance,
                hit.distance,
                Time.deltaTime * _collisionPullSpeed);
        }

        // コリジョン補正後の最終カメラ位置を算出
        Vector3 finalPos = pivot + rotation * new Vector3(0f, 0f, -adjustedDistance);

        // 位置・回転をそれぞれ Lerp/Slerp でスムージングして適用
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
