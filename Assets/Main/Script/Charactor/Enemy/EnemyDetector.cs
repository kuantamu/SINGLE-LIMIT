using UnityEngine;

/// <summary>
/// 敵のプレイヤー検知コンポーネント。
///
/// ■ 検知の仕組み
///   1. Physics.OverlapSphere（SphereCollider 相当）で半径内を判定する
///   2. 半径内に入ったオブジェクトの中から Tag "Player" を探す
///   3. さらに前方の扇状範囲内（視野角）に入っているかを確認する
///
/// ■ Inspector 設定
///   Detection Range : 検知半径（m）
///   Field Of View   : 視野角（度）。180 で前方半円、360 で全方位
///   Detection Layer : プレイヤーが属するレイヤー
///   Obstacle Layer  : 視線を遮る障害物のレイヤー（壁など）
///   Use Line Of Sight: 有効にすると壁越しに検知しない
/// </summary>
[RequireComponent(typeof(CharacterStats))]
public class EnemyDetector : MonoBehaviour
{
    [Header("検知範囲")]
    [SerializeField] private float _detectionRange = 10f;

    [Tooltip("前方からの視野角（片側）。60 なら前方 120 度の扇形になる")]
    [SerializeField] private float _fieldOfView = 60f;

    [Header("レイヤー")]
    [SerializeField] private LayerMask _detectionLayer;
    [SerializeField] private LayerMask _obstacleLayer;

    [Header("オプション")]
    [Tooltip("有効にすると壁などの障害物越しに検知しない")]
    [SerializeField] private bool _useLineOfSight = true;

    [Tooltip("検知判定の更新間隔（秒）。0 で毎フレーム更新")]
    [SerializeField] private float _updateInterval = 0.1f;

    [Tooltip("デバッグ用の Gizmo を表示する")]
    [SerializeField] private bool _drawGizmos = true;

    private CharacterStats _characterStats => gameObject.GetComponent<CharacterStats>();

    // ---- プロパティ ----

    /// <summary>現在検知しているプレイヤーの Transform（未検知なら null）。</summary>
    public Transform DetectedCharactor { get; private set; }

    public Transform AttackCharactor { get; private set; }

    /// <summary>プレイヤーを検知しているか</summary>
    public bool IsPlayerDetected => DetectedCharactor != null;

    public void ApplyNpcType(NpcTypeData type)
    {
        if (type == null) return;

        _detectionRange = type.DetectionRange;
        _fieldOfView = type.FieldOfView;
        _detectionLayer = type.DetectionLayer;
        _obstacleLayer = type.ObstacleLayer;
        _useLineOfSight = type.UseLineOfSight;
        _updateInterval = type.DetectionUpdateInterval;
    }

    // ---- 内部 ----
    private float _intervalTimer;
    private readonly Collider[] _overlapResults = new Collider[8];

    // ---- Unity ライフサイクル ----

    private void Update()
    {
        _intervalTimer -= Time.deltaTime;
        if (_intervalTimer > 0f) return;

        _intervalTimer = _updateInterval;
        UpdateDetection();
    }

    // ---- 内部処理 ----

    private void UpdateDetection()
    {
        // 半径内の collider を取得
        int count = Physics.OverlapSphereNonAlloc(
            transform.position, _detectionRange, _overlapResults, _detectionLayer);

        for (int i = 0; i < count; i++)
        {
            Collider col = _overlapResults[i];
            if (!CharacterFactionRules.IsHostile(_characterStats, col.GetComponent<CharacterStats>()))
            {
                continue;
            }

            // プレイヤーのルート Transform を取得
            // コライダーが子オブジェクトにある場合でも、プレイヤーのルート位置を計算する
            Transform target = col.transform.root;

            // 扇状視野チェック
            if (!IsInFieldOfView(target.position)) continue;

            // 視線チェック（障害物越し不可のオプション）
            if (_useLineOfSight && !HasLineOfSight(target.position)) continue;

            DetectedCharactor = target;
            return;
        }

        // 範囲内に見つからなかった → 未検知
        if(DetectedCharactor == null)
        {
            DetectedCharactor = null;
        }
        if(_characterStats.lastAttackChara != null)
        {
            AttackCharactor = _characterStats.lastAttackChara.transform;
        }
        if(AttackCharactor != null)
        {
            DetectedCharactor = AttackCharactor;
        }
    }

    /// <summary>対象が前方の扇状視野に入っているか</summary>
    private bool IsInFieldOfView(Vector3 targetPos)
    {
        Vector3 toTarget = (targetPos - transform.position);
        toTarget.y = 0f;

        // 真後ろなど方向がほぼゼロの場合はスキップ
        if (toTarget.sqrMagnitude < 0.001f) return false;

        float angle = Vector3.Angle(transform.forward, toTarget);
        return angle <= _fieldOfView;
    }

    /// <summary>対象への視線が障害物で遮られていないか</summary>
    private bool HasLineOfSight(Vector3 targetPos)
    {
        // 目の高さ（腰くらい）からレイを飛ばす
        Vector3 origin = transform.position + Vector3.up * 1.0f;
        Vector3 targetEye = targetPos + Vector3.up * 1.0f;
        Vector3 dir = (targetEye - origin);
        float dist = dir.magnitude;

        return !Physics.Raycast(origin, dir.normalized, dist, _obstacleLayer);
    }

    // ---- Gizmo（Scene ビューで視野を可視化）----

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!_drawGizmos) return;

        // 検知半径
        UnityEditor.Handles.color = IsPlayerDetected
            ? new Color(1f, 0f, 0f, 0.15f)
            : new Color(1f, 1f, 0f, 0.10f);
        UnityEditor.Handles.DrawSolidArc(
            transform.position,
            Vector3.up,
            Quaternion.Euler(0f, -_fieldOfView, 0f) * transform.forward,
            _fieldOfView * 2f,
            _detectionRange);

        // 境界線
        Gizmos.color = IsPlayerDetected ? Color.red : Color.yellow;
        Vector3 leftBound = Quaternion.Euler(0, -_fieldOfView, 0) * transform.forward;
        Vector3 rightBound = Quaternion.Euler(0, _fieldOfView, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, leftBound * _detectionRange);
        Gizmos.DrawRay(transform.position, rightBound * _detectionRange);

        // 検知中のプレイヤーへの線
        if (IsPlayerDetected)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, DetectedCharactor.position);
        }
    }
#endif
}
