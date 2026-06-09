using UnityEngine;

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

    private CharacterStats _characterStats;

    public Transform DetectedCharactor { get; private set; }

    public Transform AttackCharactor { get; private set; }

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

    private float _intervalTimer;
    private readonly Collider[] _overlapResults = new Collider[8];

    private void Awake()
    {
        _characterStats = GetComponent<CharacterStats>();
    }

    private void Update()
    {
        _intervalTimer -= Time.deltaTime;
        if (_intervalTimer > 0f) return;

        _intervalTimer = _updateInterval;
        UpdateDetection();
    }

    private void UpdateDetection()
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position, _detectionRange, _overlapResults, _detectionLayer);

        for (int i = 0; i < count; i++)
        {
            Collider col = _overlapResults[i];
            if (!CharacterFactionRules.IsHostile(_characterStats, col.GetComponent<CharacterStats>()))
                continue;

            Transform target = col.transform.root;

            if (!IsInFieldOfView(target.position)) continue;

            if (_useLineOfSight && !HasLineOfSight(target.position)) continue;

            DetectedCharactor = target;
            return;
        }

        if (_characterStats.lastAttackChara != null)
            AttackCharactor = _characterStats.lastAttackChara.transform;

        if (AttackCharactor != null)
            DetectedCharactor = AttackCharactor;
    }

    private bool IsInFieldOfView(Vector3 targetPos)
    {
        Vector3 toTarget = targetPos - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 0.001f) return false;

        return Vector3.Angle(transform.forward, toTarget) <= _fieldOfView;
    }

    private bool HasLineOfSight(Vector3 targetPos)
    {
        Vector3 origin = transform.position + Vector3.up;
        Vector3 dir = targetPos + Vector3.up - origin;

        return !Physics.Raycast(origin, dir.normalized, dir.magnitude, _obstacleLayer);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!_drawGizmos) return;

        UnityEditor.Handles.color = IsPlayerDetected
            ? new Color(1f, 0f, 0f, 0.15f)
            : new Color(1f, 1f, 0f, 0.10f);
        UnityEditor.Handles.DrawSolidArc(
            transform.position,
            Vector3.up,
            Quaternion.Euler(0f, -_fieldOfView, 0f) * transform.forward,
            _fieldOfView * 2f,
            _detectionRange);

        Gizmos.color = IsPlayerDetected ? Color.red : Color.yellow;
        Vector3 leftBound = Quaternion.Euler(0, -_fieldOfView, 0) * transform.forward;
        Vector3 rightBound = Quaternion.Euler(0, _fieldOfView, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, leftBound * _detectionRange);
        Gizmos.DrawRay(transform.position, rightBound * _detectionRange);

        if (IsPlayerDetected)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, DetectedCharactor.position);
        }
    }
#endif
}
