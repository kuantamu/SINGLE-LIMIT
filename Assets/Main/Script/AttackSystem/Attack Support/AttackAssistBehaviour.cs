using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// 攻撃アシスト（攻撃移動）の Timeline クリップ動作を定義する PlayableBehaviour。
///
/// ■ 移動先の決定ルール
///   1. ロックオン中       → ロックオン中のターゲット方向
///   2. ロックオンなし     → moveDistance 以内の最近傍の敵方向
///   3. 敵が見つからない   → キャラクターの前方
///   ※ 敵への最小接近距離（minApproachDistance）より近づかない
///
/// ■ 移動モード（useWarp）
///   false : スムーズ移動 — クリップ終了時刻に移動が完了するよう速度を毎フレーム計算
///   true  : ワープ       — クリップ開始直後に壁チェックを行い、通れれば瞬時にワープ
/// </summary>
[System.Serializable]
public class AttackAssistBehaviour : PlayableBehaviour
{
    // ─── Inspector 設定 ───────────────────────────────────────────────

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

    // ─── ランタイム変数（[System.NonSerialized] で Timeline ウィンドウに非表示）─

    [System.NonSerialized] private Vector3 _destination;
    [System.NonSerialized] private float   _clipDuration;
    [System.NonSerialized] private bool    _initialized;
    [System.NonSerialized] private bool    _warpDone;
    [System.NonSerialized] private bool    _moveDone;

    /// <summary>OnBehaviourPause でも StopAssist を呼べるようキャッシュする。</summary>
    [System.NonSerialized] private AttackAssistController _cachedController;

    // ─── PlayableBehaviour ──────────────────────────────────────────

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        // クリップが再び再生されたときにリセット
        _initialized = false;
        _warpDone    = false;
        _moveDone    = false;
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (!Application.isPlaying) return;

        _cachedController = playerData as AttackAssistController;
        if (_cachedController == null) return;

        // 初回フレームで目的地を確定する
        if (!_initialized)
        {
            _clipDuration = (float)playable.GetDuration();
            _destination  = CalculateDestination(_cachedController);
            _initialized  = true;
        }

        if (useWarp)
        {
            // ワープは1回だけ実行
            if (!_warpDone)
            {
                ApplyWarp(_cachedController);
                _warpDone = true;
            }
        }
        else
        {
            // スムーズ移動：到達済みなら何もしない
            if (!_moveDone)
                ApplySmoothMove(_cachedController, playable);
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (!Application.isPlaying) return;

        // スムーズ移動モードではクリップ終了時に速度をリセットする
        if (!useWarp)
            _cachedController?.StopAssist();
    }

    // ─── 目的地の計算 ────────────────────────────────────────────────

    /// <summary>
    /// 移動先ワールド座標を決定する。
    /// ロックオン → 範囲内の最近傍敵 → 前方 の優先順で方向を決め、
    /// minApproachDistance を守りながら moveDistance を上限に移動先を算出する。
    /// </summary>
    private Vector3 CalculateDestination(AttackAssistController controller)
    {
        Vector3    origin = controller.transform.position;
        Transform  enemy  = null;

        // 1. ロックオン中
        if (controller.LockOnController != null && controller.LockOnController.IsLockedOn)
        {
            enemy = controller.LockOnController.CurrentTarget;
        }
        // 2. ロックオンなし → moveDistance 以内の最近傍敵
        else
        {
            enemy = FindNearestEnemyInRange(controller, moveDistance);
        }

        if (enemy != null)
        {
            return CalcDestinationToEnemy(origin, enemy);
        }

        // 3. 敵なし → 前方に moveDistance 移動
        Vector3 forward = controller.transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
        return origin + forward.normalized * moveDistance;
    }

    /// <summary>敵位置と minApproachDistance を考慮した移動先を返す。</summary>
    private Vector3 CalcDestinationToEnemy(Vector3 origin, Transform enemy)
    {
        Vector3 toEnemy = enemy.position - origin;
        toEnemy.y = 0f;

        float horizontalDist = toEnemy.magnitude;

        // 0 除算を防ぐ
        if (horizontalDist < 0.001f)
            return origin;

        Vector3 dir       = toEnemy / horizontalDist;
        float   maxMove   = Mathf.Max(0f, horizontalDist - minApproachDistance);
        float   actualMove = Mathf.Min(moveDistance, maxMove);

        return origin + dir * actualMove;
    }

    /// <summary>
    /// 指定距離以内の最近傍の有効な敵を返す。
    /// まず LockOnController のキャッシュを参照し、なければ OverlapSphere で直接検索する。
    /// </summary>
    private Transform FindNearestEnemyInRange(AttackAssistController controller, float range)
    {
        // LockOnController のキャッシュを優先利用（毎フレーム OverlapSphere を避ける）
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

        // フォールバック：直接 OverlapSphere で検索
        // ※ 攻撃開始時の1回のみ呼ばれるため GC 割り当ては許容する
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

    // ─── スムーズ移動 ────────────────────────────────────────────────

    /// <summary>
    /// クリップの残り時間から必要な速度を毎フレーム逆算し SetAssistVelocity で適用する。
    /// これにより「クリップ終了時に移動が完了する」動きを実現する。
    /// </summary>
    private void ApplySmoothMove(AttackAssistController controller, Playable playable)
    {
        float elapsed   = (float)playable.GetTime();
        float remaining = _clipDuration - elapsed;

        // 残り時間が 1 フレーム未満なら停止して完了
        if (remaining <= Time.deltaTime)
        {
            controller.StopAssist();
            _moveDone = true;
            return;
        }

        Vector3 delta = _destination - controller.transform.position;
        delta.y = 0f;

        float dist = delta.magnitude;

        // 目的地に十分近づいたら停止
        if (dist < 0.02f)
        {
            controller.StopAssist();
            _moveDone = true;
            return;
        }

        // 速度 = 残り距離 ÷ 残り時間
        float   speed    = dist / remaining;
        Vector3 velocity = delta.normalized * speed;
        controller.SetAssistVelocity(velocity);
    }

    // ─── ワープ ──────────────────────────────────────────────────────

    /// <summary>
    /// 目的地方向へ SphereCast で壁チェックを行い、
    /// 通れる場合のみ Rigidbody.MovePosition で瞬時に移動する。
    /// </summary>
    private void ApplyWarp(AttackAssistController controller)
    {
        Vector3 origin = controller.transform.position;
        Vector3 dir    = _destination - origin;
        dir.y = 0f;

        float dist = dir.magnitude;
        if (dist < 0.01f) return;

        // SphereCast で壁（wallLayer）との衝突を確認
        Vector3 castOrigin = origin + Vector3.up * wallCheckRadius;
        bool    blocked    = Physics.SphereCast(
            castOrigin, wallCheckRadius, dir.normalized,
            out _, dist, wallLayer);

        if (blocked) return;

        // Y 座標はワープ先でもプレイヤーの現在高さを維持する
        Vector3 warpPos = new Vector3(_destination.x, origin.y, _destination.z);
        controller.WarpTo(warpPos);
    }
}
