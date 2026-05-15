using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class LockOnController : MonoBehaviour
{
    [Header("Lock On")]
    [SerializeField] private KeyCode _lockOnKey = KeyCode.R;
    [SerializeField] private float _lockOnRange = 12f;
    [SerializeField] private LayerMask _enemyLayer = 1 << 8;
    [SerializeField] private LayerMask _obstacleLayer;
    [SerializeField] private bool _useLineOfSight = true;
    [SerializeField] private float _refreshInterval = 0.05f;

    [Header("References")]
    [SerializeField] private ThirdPersonCamera _thirdPersonCamera;
    [SerializeField] private Camera _mainCamera;

    [Header("Debug")]
    [SerializeField] private bool _drawGizmos = true;

    public Transform CurrentTarget { get; private set; }
    public Transform NearestEnemy { get; private set; }
    public bool IsLockedOn => CurrentTarget != null;

    private const int MaxTargets = 32;
    private readonly Collider[] _overlapResults = new Collider[MaxTargets];
    private readonly List<Transform> _targets = new List<Transform>(MaxTargets);
    private SphereCollider _rangeCollider;
    private float _refreshTimer;

    private void Awake()
    {
        _rangeCollider = GetComponent<SphereCollider>();
        _rangeCollider.isTrigger = true;
        _rangeCollider.radius = _lockOnRange;

        if (_mainCamera == null)
            _mainCamera = Camera.main;

        if (_thirdPersonCamera == null && _mainCamera != null)
            _thirdPersonCamera = _mainCamera.GetComponent<ThirdPersonCamera>();
    }

    private void Update()
    {
        SyncColliderRadius();
        TickTargets();

        if (Input.GetKeyDown(_lockOnKey))
            ToggleLockOn();

        if (CurrentTarget != null && !IsLockOnTargetValid(CurrentTarget))
            ClearLockOn(resetCamera: true);
    }

    public Transform FindNearestEnemy(bool requireVisible)
    {
        RefreshTargets();

        Transform nearest = null;
        float nearestSqrDistance = float.MaxValue;

        for (int i = 0; i < _targets.Count; i++)
        {
            Transform target = _targets[i];
            if (requireVisible && !IsVisible(target)) continue;

            float sqrDistance = (target.position - transform.position).sqrMagnitude;
            if (sqrDistance >= nearestSqrDistance) continue;

            nearestSqrDistance = sqrDistance;
            nearest = target;
        }

        return nearest;
    }

    public void ClearLockOn(bool resetCamera)
    {
        CurrentTarget = null;

        if (_thirdPersonCamera != null)
            _thirdPersonCamera.ClearLockOn(resetCamera);
    }

    private void ToggleLockOn()
    {
        if (CurrentTarget != null)
        {
            ClearLockOn(resetCamera: true);
            return;
        }

        Transform target = FindScreenCenterTarget();
        if (target == null)
        {
            ClearLockOn(resetCamera: true);
            return;
        }

        CurrentTarget = target;
        if (_thirdPersonCamera != null)
            _thirdPersonCamera.SetLockOnTarget(CurrentTarget);
    }

    private void TickTargets()
    {
        _refreshTimer -= Time.deltaTime;
        if (_refreshTimer > 0f) return;

        _refreshTimer = _refreshInterval;
        RefreshTargets();
    }

    private void RefreshTargets()
    {
        _targets.Clear();

        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            _lockOnRange,
            _overlapResults,
            _enemyLayer);

        NearestEnemy = null;
        float nearestSqrDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Transform target = LockOnTargetUtility.GetTargetRoot(_overlapResults[i]);
            if (!LockOnTargetUtility.IsValidEnemy(target)) continue;
            if (_targets.Contains(target)) continue;

            _targets.Add(target);

            float sqrDistance = (target.position - transform.position).sqrMagnitude;
            if (sqrDistance >= nearestSqrDistance) continue;

            nearestSqrDistance = sqrDistance;
            NearestEnemy = target;
        }
    }

    private Transform FindScreenCenterTarget()
    {
        RefreshTargets();

        Transform bestTarget = null;
        float bestScore = float.MaxValue;

        for (int i = 0; i < _targets.Count; i++)
        {
            Transform target = _targets[i];
            if (!IsVisible(target)) continue;

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

    private bool IsLockOnTargetValid(Transform target)
    {
        if (!LockOnTargetUtility.IsValidEnemy(target)) return false;
        if ((target.position - transform.position).sqrMagnitude > _lockOnRange * _lockOnRange) return false;

        return IsVisible(target);
    }

    private bool IsVisible(Transform target)
    {
        if (_mainCamera == null || target == null) return false;

        Vector3 aimPoint = LockOnTargetUtility.GetAimPoint(target);
        Vector3 viewport = _mainCamera.WorldToViewportPoint(aimPoint);

        if (viewport.z <= 0f) return false;
        if (viewport.x < 0f || viewport.x > 1f) return false;
        if (viewport.y < 0f || viewport.y > 1f) return false;

        if (!_useLineOfSight) return true;

        Vector3 origin = _mainCamera.transform.position;
        Vector3 direction = aimPoint - origin;
        float distance = direction.magnitude;
        if (distance < 0.001f) return true;

        return !Physics.Raycast(origin, direction.normalized, distance, _obstacleLayer);
    }

    private void SyncColliderRadius()
    {
        if (_rangeCollider != null && !Mathf.Approximately(_rangeCollider.radius, _lockOnRange))
            _rangeCollider.radius = _lockOnRange;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!_drawGizmos) return;

        Gizmos.color = CurrentTarget != null
            ? new Color(0.2f, 0.7f, 1f, 0.25f)
            : new Color(0.2f, 1f, 0.4f, 0.15f);
        Gizmos.DrawSphere(transform.position, _lockOnRange);

        if (CurrentTarget == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, LockOnTargetUtility.GetAimPoint(CurrentTarget));
    }
#endif
}
