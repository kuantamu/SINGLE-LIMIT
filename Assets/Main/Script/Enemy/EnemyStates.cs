using UnityEngine;

// ============================================================
// EnemyIdleState
// ============================================================

/// <summary>
/// 待機ステート。
/// EnemyDetector がプレイヤーを検知したら Chase へ移行する。
/// </summary>
public class EnemyIdleState : EnemyState
{
    public EnemyIdleState(EnemyStateMachine sm) : base(sm) {}

    public override void Enter()
    {
        SM.Movement.StopHorizontal();
        SM.AnimController.PlayIdle();
    }

    public override void Update()
    {
        if (SM.Detector.IsPlayerDetected)
            SM.TransitionTo(SM.Chase);
    }
}

// ============================================================
// EnemyChaseState
// ============================================================

/// <summary>
/// 追跡ステート。
/// 検知が途切れたら Idle に戻る。攻撃範囲内に入ったら Attack へ。
/// </summary>
public class EnemyChaseState : EnemyState
{
    public EnemyChaseState(EnemyStateMachine sm) : base(sm) {}

    public override void Enter()
    {
        SM.AnimController.PlayChase();
    }

    public override void Update()
    {
        // 検知が途切れた → Idle
        if (!SM.Detector.IsPlayerDetected)
        {
            SM.TransitionTo(SM.Idle);
            return;
        }

        // 攻撃範囲内 → Attack
        if (SM.DistanceToPlayer <= SM.AttackRange)
        {
            SM.TransitionTo(SM.Attack);
            return;
        }

        SM.Movement.MoveToward(SM.PlayerTransform.position);
    }
}

// ============================================================
// EnemyAttackState
// ============================================================

/// <summary>
/// 攻撃ステート。
/// モーション終了後インターバルを挟んで Chase か Attack に戻る。
/// </summary>
public class EnemyAttackState : EnemyState
{
    private bool  _motionEnded;
    private float _intervalTimer;

    public EnemyAttackState(EnemyStateMachine sm) : base(sm) {}

    public override void Enter()
    {
        _motionEnded   = false;
        _intervalTimer = 0f;

        SM.Movement.StopHorizontal();

        SM.AnimController.OnMotionEnd    += HandleMotionEnd;
        SM.AnimController.OnAttackActive += HandleAttackActive;
        SM.AnimController.OnAttackEnd    += HandleAttackEnd;

        int index = Random.Range(0, SM.AnimController.AttackCount);
        SM.AnimController.PlayAttack(index);
    }

    public override void Exit()
    {
        SM.AnimController.OnMotionEnd    -= HandleMotionEnd;
        SM.AnimController.OnAttackActive -= HandleAttackActive;
        SM.AnimController.OnAttackEnd    -= HandleAttackEnd;
    }

    public override void Update()
    {
        if (SM.PlayerTransform != null)
            SM.Movement.FaceToward(SM.PlayerTransform.position);

        if (!_motionEnded) return;

        _intervalTimer -= Time.deltaTime;
        if (_intervalTimer <= 0f)
        {
            if (SM.DistanceToPlayer > SM.AttackRange)
                SM.TransitionTo(SM.Chase);
            else
                SM.TransitionTo(SM.Attack);
        }
    }

    private void HandleAttackActive()
    {
        // TODO: ヒットボックスを有効化
    }

    private void HandleAttackEnd()
    {
        // TODO: ヒットボックスを無効化
    }

    private void HandleMotionEnd()
    {
        _motionEnded   = true;
        _intervalTimer = SM.AttackInterval;
    }
}

// ============================================================
// EnemyStaggerState
// ============================================================

/// <summary>
/// 怯みステート。モーション終了後 Chase に戻る。
/// </summary>
public class EnemyStaggerState : EnemyState
{
    public EnemyStaggerState(EnemyStateMachine sm) : base(sm) {}

    public override void Enter()
    {
        SM.Movement.StopHorizontal();
        SM.AnimController.OnMotionEnd += HandleMotionEnd;
        SM.AnimController.PlayStagger();
    }

    public override void Exit()
    {
        SM.AnimController.OnMotionEnd -= HandleMotionEnd;
    }

    public override void Update() {}

    private void HandleMotionEnd() => SM.TransitionTo(SM.Chase);
}

// ============================================================
// EnemyDeathState
// ============================================================

/// <summary>
/// 敵の死亡ステート。
/// 死亡モーション再生後に GameObject を削除する。
/// CharacterStats.OnDeath イベントから EnemyStateMachine.TriggerDeath() 経由で遷移する。
/// </summary>
public class EnemyDeathState : EnemyState
{
    public EnemyDeathState(EnemyStateMachine sm) : base(sm) {}

    public override void Enter()
    {
        SM.Movement.StopHorizontal();

        SM.AnimController.OnMotionEnd += HandleMotionEnd;
        SM.AnimController.PlayDeath();
    }

    public override void Exit()
    {
        SM.AnimController.OnMotionEnd -= HandleMotionEnd;
    }

    public override void Update() {}

    private void HandleMotionEnd()
    {
        UnityEngine.Object.Destroy(SM.gameObject);
    }
}

// ============================================================
// EnemyKnockbackState
// ============================================================

/// <summary>
/// ノックバックステート。
/// Stagger モーションを流用しつつ、EnemyMovement.StartKnockback() で移動させる。
/// モーション終了またはノックバック移動完了後に Chase に戻る。
/// KnockbackHitEffect → EnemyStateMachine.TriggerKnockback() 経由で遷移する。
/// </summary>
public class EnemyKnockbackState : EnemyState
{
    private Vector3 _dir;
    private float   _distance;
    private float   _duration;

    public EnemyKnockbackState(EnemyStateMachine sm) : base(sm) {}

    /// <summary>遷移前にノックバックのパラメータをセットする。</summary>
    public void SetKnockback(Vector3 dir, float distance, float duration)
    {
        _dir      = dir;
        _distance = distance;
        _duration = duration;
    }

    public override void Enter()
    {
        SM.Movement.StartKnockback(_dir, _distance, _duration);
        SM.AnimController.OnMotionEnd += HandleMotionEnd;
        SM.AnimController.PlayStagger(); // Stagger モーションを流用
    }

    public override void Exit()
    {
        SM.AnimController.OnMotionEnd -= HandleMotionEnd;
        SM.Movement.StopKnockback();
    }

    public override void Update() {} // ノックバック中は入力を受け付けない

    private void HandleMotionEnd() => SM.TransitionTo(SM.Chase);
}
