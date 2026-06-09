using UnityEngine;

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

public class EnemyChaseState : EnemyState
{
    public EnemyChaseState(EnemyStateMachine sm) : base(sm) {}

    public override void Enter()
    {
        SM.AnimController.PlayChase();
    }

    public override void Update()
    {
        if (!SM.Detector.IsPlayerDetected)
        {
            SM.TransitionTo(SM.Idle);
            return;
        }

        if (SM.DistanceToPlayer <= SM.AttackRange)
        {
            SM.TransitionTo(SM.Attack);
            return;
        }

        SM.Movement.MoveToward(SM.PlayerTransform.position);
    }
}

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
    }

    private void HandleAttackEnd()
    {
    }

    private void HandleMotionEnd()
    {
        _motionEnded   = true;
        _intervalTimer = SM.AttackInterval;
    }
}

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

public class EnemyKnockbackState : EnemyState
{
    private Vector3 _dir;
    private float   _distance;
    private float   _duration;

    public EnemyKnockbackState(EnemyStateMachine sm) : base(sm) {}

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
