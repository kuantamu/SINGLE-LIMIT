using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class AttackAssistBehaviour : PlayableBehaviour
{

    [Header("移動設定")]
    [Tooltip("移動量（メートル）")]
    public float moveDistance = 2f;

    [Tooltip("敵への最小接近距離。これより近くには移動しない（メートル）")]
    public float minApproachDistance = 1.2f;

    [Header("移動モード")]
    [Tooltip("false = スムーズ移動 / true = 壁がなければワープ")]
    public bool useWarp = false;

    [Header("ワープ設定（useWarp = true のみ）")]
    [Tooltip("壁と判定するレイヤーマスク")]
    public LayerMask wallLayer;

    [Tooltip("壁チェック用 SphereCast の半径（メートル）")]
    public float wallCheckRadius = 0.3f;

    [System.NonSerialized] private Vector3 _destination;
    [System.NonSerialized] private float   _clipDuration;
    [System.NonSerialized] private bool    _initialized;
    [System.NonSerialized] private bool    _warpDone;
    [System.NonSerialized] private bool    _moveDone;

    [System.NonSerialized] private AttackAssistController _cachedController;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        _initialized = false;
        _warpDone    = false;
        _moveDone    = false;
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (!Application.isPlaying) return;

        _cachedController = playerData as AttackAssistController;
        if (_cachedController == null) return;

        if (!_initialized)
        {
            _clipDuration = (float)playable.GetDuration();
            _destination  = CalculateDestination(_cachedController);
            _initialized  = true;
        }

        if (useWarp)
        {
            if (!_warpDone)
            {
                ApplyWarp(_cachedController);
                _warpDone = true;
            }
        }
        else
        {
            if (!_moveDone)
                ApplySmoothMove(_cachedController, playable);
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (!Application.isPlaying) return;

        if (!useWarp)
            _cachedController?.StopAssist();
    }

    private Vector3 CalculateDestination(AttackAssistController controller)
    {
        Vector3    origin = controller.transform.position;
        Transform  enemy  = null;

        if (controller.LockOnController != null && controller.LockOnController.IsLockedOn)
        {
            enemy = controller.LockOnController.CurrentTarget;
        }
        else
        {
            enemy = FindNearestEnemyInRange(controller, moveDistance);
        }

        if (enemy != null)
        {
            return CalcDestinationToEnemy(origin, enemy);
        }

        Vector3 forward = controller.transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
        return origin + forward.normalized * moveDistance;
    }

    private Vector3 CalcDestinationToEnemy(Vector3 origin, Transform enemy)
    {
        Vector3 toEnemy = enemy.position - origin;
        toEnemy.y = 0f;

        float horizontalDist = toEnemy.magnitude;

        if (horizontalDist < 0.001f)
            return origin;

        Vector3 dir       = toEnemy / horizontalDist;
        float   maxMove   = Mathf.Max(0f, horizontalDist - minApproachDistance);
        float   actualMove = Mathf.Min(moveDistance, maxMove);

        return origin + dir * actualMove;
    }

    private Transform FindNearestEnemyInRange(AttackAssistController controller, float range)
    {
        if (controller.LockOnController != null)
        {
            Transform nearest = controller.LockOnController.NearestEnemy;
            if (nearest != null)
            {
                Vector3 diff = nearest.position - controller.transform.position;
                diff.y = 0f;
                if (diff.magnitude <= range)
                    return nearest;
            }
        }

        Collider[] hits = Physics.OverlapSphere(
            controller.transform.position, range, controller.EnemyDetectionLayer);

        Transform best        = null;
        float     bestSqrDist = float.MaxValue;

        foreach (Collider col in hits)
        {
            Transform t = LockOnTargetUtility.GetTargetRoot(col);
            if (!LockOnTargetUtility.IsValidEnemy(t)) continue;

            float sqrDist = (t.position - controller.transform.position).sqrMagnitude;
            if (sqrDist >= bestSqrDist) continue;

            bestSqrDist = sqrDist;
            best        = t;
        }

        return best;
    }

    private void ApplySmoothMove(AttackAssistController controller, Playable playable)
    {
        float elapsed   = (float)playable.GetTime();
        float remaining = _clipDuration - elapsed;

        if (remaining <= Time.deltaTime)
        {
            controller.StopAssist();
            _moveDone = true;
            return;
        }

        Vector3 delta = _destination - controller.transform.position;
        delta.y = 0f;

        float dist = delta.magnitude;

        if (dist < 0.02f)
        {
            controller.StopAssist();
            _moveDone = true;
            return;
        }

        float   speed    = dist / remaining;
        Vector3 velocity = delta.normalized * speed;
        controller.SetAssistVelocity(velocity);
    }

    private void ApplyWarp(AttackAssistController controller)
    {
        Vector3 origin = controller.transform.position;
        Vector3 dir    = _destination - origin;
        dir.y = 0f;

        float dist = dir.magnitude;
        if (dist < 0.01f) return;

        Vector3 castOrigin = origin + Vector3.up * wallCheckRadius;
        bool    blocked    = Physics.SphereCast(
            castOrigin, wallCheckRadius, dir.normalized,
            out _, dist, wallLayer);

        if (blocked) return;

        Vector3 warpPos = new Vector3(_destination.x, origin.y, _destination.z);
        controller.WarpTo(warpPos);
    }
}
