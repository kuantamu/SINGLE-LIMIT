using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーのロックオン（ターゲット追跡）を管理するコントローラー。
/// SphereCollider を使って範囲内の敵を常時リストアップし、
/// キー入力でスクリーン中央に最も近い敵へロックオンする。
/// ロックオン中は ThirdPersonCamera にターゲットを伝え、カメラが自動追従する。
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class LockOnController : MonoBehaviour
{
    #region 変数
    #region 設定

    [Header("Lock On")]
    /// <summary>ロックオンを切り替えるキー（デフォルト: R）</summary>
    [SerializeField] private KeyCode _lockOnKey = KeyCode.R;

    /// <summary>ロックオン可能な最大距離（メートル）</summary>
    [SerializeField] private float _lockOnRange = 12f;

    /// <summary>敵を検出するレイヤーマスク（デフォルト: Layer 8）</summary>
    [SerializeField] private LayerMask _enemyLayer = 1 << 8;

    /// <summary>視線チェックで障害物とみなすレイヤーマスク</summary>
    [SerializeField] private LayerMask _obstacleLayer;

    /// <summary>true のとき、壁などで遮られた敵はロックオン対象から除外する</summary>
    [SerializeField] private bool _useLineOfSight = true;

    /// <summary>ターゲットリストの更新間隔（秒）。毎フレーム更新しないことで負荷を抑える</summary>
    [SerializeField] private float _refreshInterval = 0.05f;

    [Header("References")]
    /// <summary>カメラ制御スクリプト（ロックオン時の向き変更に使用）</summary>
    [SerializeField] private ThirdPersonCamera _thirdPersonCamera;

    /// <summary>画面座標変換に使用するメインカメラ</summary>
    [SerializeField] private Camera _mainCamera;

    [Header("Debug")]
    /// <summary>true のとき、エディタ上でロックオン範囲と照準線をギズモ表示する</summary>
    [SerializeField] private bool _drawGizmos = true;
    #endregion
    #region 変数フィールド
    /// <summary>現在ロックオン中のターゲット Transform（ロックオンしていなければ null）</summary>
    public Transform CurrentTarget { get; private set; }

    /// <summary>範囲内で最も近い敵の Transform（ロックオンに関わらず更新される）</summary>
    public Transform NearestEnemy { get; private set; }

    /// <summary>現在ロックオン中かどうか</summary>
    public bool IsLockedOn => CurrentTarget != null;

    /// <summary>OverlapSphereNonAlloc 用の結果バッファ（GC を抑えるため固定長配列）</summary>
    private const int MaxTargets = 32;
    private readonly Collider[] _overlapResults = new Collider[MaxTargets];

    /// <summary>有効な敵ターゲットのキャッシュリスト</summary>
    private readonly List<Transform> _targets = new List<Transform>(MaxTargets);

    /// <summary>ロックオン範囲を表す SphereCollider（Trigger 設定）</summary>
    private SphereCollider _rangeCollider;

    /// <summary>ターゲットリストの更新タイマー</summary>
    private float _refreshTimer;
    #endregion
    #endregion

    private void Awake()
    {
        // SphereCollider をトリガーとして初期化し、半径をロックオン範囲に設定
        _rangeCollider = GetComponent<SphereCollider>();
        _rangeCollider.isTrigger = true;
        _rangeCollider.radius = _lockOnRange;

        // カメラが未設定ならメインカメラを自動取得
        if (_mainCamera == null)
            _mainCamera = Camera.main;

        // ThirdPersonCamera が未設定ならメインカメラのコンポーネントから自動取得
        if (_thirdPersonCamera == null && _mainCamera != null)
            _thirdPersonCamera = _mainCamera.GetComponent<ThirdPersonCamera>();
    }

    private void Update()
    {
        // コライダー半径を _lockOnRange と常に同期する
        SyncColliderRadius();

        // 一定間隔でターゲットリストを更新する
        TickTargets();

        // ロックオンキーが押されたらトグル
        if (Input.GetKeyDown(_lockOnKey))
            ToggleLockOn();

        // ロックオン中のターゲットが無効になったら自動解除
        if (CurrentTarget != null && !IsLockOnTargetValid(CurrentTarget))
            ClearLockOn(resetCamera: true);
    }

    /// <summary>
    /// 範囲内の敵から最も近い1体を返す。
    /// </summary>
    /// <param name="requireVisible">true のとき視野内（カメラ画面内 + 視線通過）の敵のみ対象</param>
    /// <returns>最近傍の敵 Transform（いなければ null）</returns>
    public Transform FindNearestEnemy(bool requireVisible)
    {
        RefreshTargets();

        Transform nearest = null;
        float nearestSqrDistance = float.MaxValue;

        for (int i = 0; i < _targets.Count; i++)
        {
            Transform target = _targets[i];

            // 視線フィルタが有効で見えない場合はスキップ
            if (requireVisible && !IsVisible(target)) continue;

            float sqrDistance = (target.position - transform.position).sqrMagnitude;
            if (sqrDistance >= nearestSqrDistance) continue;

            nearestSqrDistance = sqrDistance;
            nearest = target;
        }

        return nearest;
    }

    /// <summary>
    /// ロックオンを解除し、必要に応じてカメラをリセットする。
    /// </summary>
    /// <param name="resetCamera">true のとき ThirdPersonCamera にもリセットを通知する</param>
    public void ClearLockOn(bool resetCamera)
    {
        CurrentTarget = null;

        if (_thirdPersonCamera != null)
            _thirdPersonCamera.ClearLockOn(resetCamera);
    }

    // --- プライベートメソッド ---

    /// <summary>
    /// ロックオンのトグル処理。
    /// ロックオン中なら解除、未ロックオンなら画面中央に最も近い敵を選択する。
    /// </summary>
    private void ToggleLockOn()
    {
        // ロックオン中なら解除して終了
        if (CurrentTarget != null)
        {
            ClearLockOn(resetCamera: true);
            return;
        }

        // 画面中央に最も近い敵を検索
        Transform target = FindScreenCenterTarget();
        if (target == null)
        {
            ClearLockOn(resetCamera: true);
            return;
        }

        // ターゲットをセットしカメラに通知
        CurrentTarget = target;
        if (_thirdPersonCamera != null)
            _thirdPersonCamera.SetLockOnTarget(CurrentTarget);
    }

    /// <summary>
    /// 定期的にターゲットリストを更新するタイマー処理。
    /// _refreshInterval 秒ごとに RefreshTargets を呼ぶ。
    /// </summary>
    private void TickTargets()
    {
        _refreshTimer -= Time.deltaTime;
        if (_refreshTimer > 0f) return;

        _refreshTimer = _refreshInterval;
        RefreshTargets();
    }

    /// <summary>
    /// OverlapSphere でロックオン範囲内の敵を再検索し、
    /// _targets リストと NearestEnemy を最新の状態に更新する。
    /// </summary>
    private void RefreshTargets()
    {
        _targets.Clear();

        // 非アロケート版の OverlapSphere で GC を抑えて敵コライダーを取得
        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            _lockOnRange,
            _overlapResults,
            _enemyLayer);

        NearestEnemy = null;
        float nearestSqrDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            // コライダーから敵のルート Transform を取得
            Transform target = LockOnTargetUtility.GetTargetRoot(_overlapResults[i]);

            // 無効な敵や重複はスキップ
            if (!LockOnTargetUtility.IsValidEnemy(target)) continue;
            if (_targets.Contains(target)) continue;

            _targets.Add(target);

            // NearestEnemy を更新（最近傍を逐次比較）
            float sqrDistance = (target.position - transform.position).sqrMagnitude;
            if (sqrDistance >= nearestSqrDistance) continue;

            nearestSqrDistance = sqrDistance;
            NearestEnemy = target;
        }
    }

    /// <summary>
    /// 画面中央（Viewport 0.5, 0.5）に最も近い可視敵を選択する。
    /// ターゲット選択時のデフォルト動作。
    /// </summary>
    /// <returns>スクリーン中心に最も近い敵の Transform（いなければ null）</returns>
    private Transform FindScreenCenterTarget()
    {
        RefreshTargets();

        Transform bestTarget = null;
        float bestScore = float.MaxValue;

        for (int i = 0; i < _targets.Count; i++)
        {
            Transform target = _targets[i];

            // 画面外・視線が通らない敵はスキップ
            if (!IsVisible(target)) continue;

            // Viewport 座標に変換し、画面中央（0.5, 0.5）からのオフセットを距離スコアとする
            Vector3 viewport = _mainCamera.WorldToViewportPoint(
                LockOnTargetUtility.GetAimPoint(target));
            Vector2 screenOffset = new Vector2(viewport.x - 0.5f, viewport.y - 0.5f);
            float score = screenOffset.sqrMagnitude;

            if (score >= bestScore) continue;

            bestScore = score;
            bestTarget = target;
        }

        return bestTarget;
    }

    /// <summary>
    /// ロックオン中のターゲットが引き続き有効かを検証する。
    /// 死亡・範囲外・視線不通のいずれかで false を返す。
    /// </summary>
    /// <param name="target">検証するターゲット</param>
    /// <returns>引き続きロックオン可能なら true</returns>
    private bool IsLockOnTargetValid(Transform target)
    {
        if (!LockOnTargetUtility.IsValidEnemy(target)) return false;

        // sqrMagnitude で sqrt を避けて距離チェック（最適化）
        if ((target.position - transform.position).sqrMagnitude > _lockOnRange * _lockOnRange) return false;

        return IsVisible(target);
    }

    /// <summary>
    /// ターゲットがカメラの視野内にあり、かつ視線が通っているかを判定する。
    /// _useLineOfSight が false の場合は画面内チェックのみ行う。
    /// </summary>
    /// <param name="target">可視判定を行う Transform</param>
    /// <returns>見えていれば true</returns>
    private bool IsVisible(Transform target)
    {
        if (_mainCamera == null || target == null) return false;

        Vector3 aimPoint = LockOnTargetUtility.GetAimPoint(target);
        Vector3 viewport = _mainCamera.WorldToViewportPoint(aimPoint);

        // カメラの背後にある場合は非可視
        if (viewport.z <= 0f) return false;

        // 画面の範囲外は非可視（0〜1 の外なら画面外）
        if (viewport.x < 0f || viewport.x > 1f) return false;
        if (viewport.y < 0f || viewport.y > 1f) return false;

        // 視線チェックが無効なら画面内にいれば可視とみなす
        if (!_useLineOfSight) return true;

        // カメラから照準点へ Raycast を飛ばし、障害物があれば非可視と判定
        Vector3 origin = _mainCamera.transform.position;
        Vector3 direction = aimPoint - origin;
        float distance = direction.magnitude;
        if (distance < 0.001f) return true;  // ほぼ同位置なら可視

        return !Physics.Raycast(origin, direction.normalized, distance, _obstacleLayer);
    }

    /// <summary>
    /// Inspector で _lockOnRange が変更された場合にコライダー半径を同期する。
    /// ランタイム中に値を変えても SphereCollider に反映される。
    /// </summary>
    private void SyncColliderRadius()
    {
        if (_rangeCollider != null && !Mathf.Approximately(_rangeCollider.radius, _lockOnRange))
            _rangeCollider.radius = _lockOnRange;
    }

#if UNITY_EDITOR
    /// <summary>
    /// エディタ上でロックオン範囲と照準線をギズモ表示する。
    /// ロックオン中は青色、未ロックオン時は緑色の半透明球を表示する。
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!_drawGizmos) return;

        // ロックオン中: 青、未ロックオン: 緑の半透明球
        Gizmos.color = CurrentTarget != null
            ? new Color(0.2f, 0.7f, 1f, 0.25f)
            : new Color(0.2f, 1f, 0.4f, 0.15f);
        Gizmos.DrawSphere(transform.position, _lockOnRange);

        // ロックオン中のみ照準線を描画
        if (CurrentTarget == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, LockOnTargetUtility.GetAimPoint(CurrentTarget));
    }
#endif
}
