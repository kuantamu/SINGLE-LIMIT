using UnityEngine;

public class IdleState : PlayerState
{
    public IdleState(PlayerStateMachine sm) : base(sm) {}

    public override void Update()
    {
        if (SM.InputHandler.SpecialPressed)
        {
            SM.InputHandler.CancelBuffer();
            SM.TransitionTo(SM.Special);
            return;
        }

        if (SM.InputHandler.AttackPressed)
        {
            SM.InputHandler.CancelBuffer();
            SM.Attack.SetComboIndex(0);
            SM.TransitionTo(SM.Attack);
            return;
        }
        if (SM.InputHandler.HeavyAttackPressed)
        {
            SM.TransitionTo(SM.HeavyAttack);
            return;
        }

        if (SM.InputHandler.IsGuardHeld)
        {
            SM.InputHandler.CancelBuffer();
            SM.TransitionTo(SM.Guard);
            return;
        }

        SM.Movement.Move(SM.InputHandler.MoveInput);
        SM.Movement.FastSpeed(SM.InputHandler.IsFastSpeedHeld);
        if (SM.InputHandler.IsMoving)
            SM.AnimController.PlayMove();
        else
            SM.AnimController.PlayIdle();
    }
}

public class GuardState : PlayerState
{
    private CharacterStats _stats;

    public GuardState(PlayerStateMachine sm) : base(sm) {}

    public override void Enter()
    {
        SM.Movement.StopHorizontal();
        SM.AnimController.PlayGuard();

        _stats = SM.CharStats;
        if (_stats != null)
            _stats.IsGuarding = true;
    }

    public override void Exit()
    {
        if (_stats != null)
            _stats.IsGuarding = false;
    }

    public override void Update()
    {
        if (SM.InputHandler.AttackPressed)
        {
            SM.InputHandler.CancelBuffer();
            SM.Attack.SetComboIndex(0);
            SM.TransitionTo(SM.Attack);
            return;
        }
        if (SM.InputHandler.HeavyAttackPressed)
        {
            SM.TransitionTo(SM.HeavyAttack);
            return;
        }

        if (!SM.InputHandler.IsGuardHeld)
        {
            SM.TransitionTo(SM.Idle);
            return;
        }
        
        SM.Movement.FaceCamera();
        SM.Movement.GuardMove(SM.InputHandler.MoveInput);
        if (SM.InputHandler.IsDodgePush)
        {
            SM.InputHandler.CancelBuffer();
            SM.TransitionTo(SM.Dodge);
            if (!SM.InputHandler.IsMoving)
                SM.Dodge.RequestSpotDodge();

            return;
        }
    }
}

public class SpecialState : PlayerState
{
    public SpecialState(PlayerStateMachine sm) : base(sm){}

    public override void Enter()
    {
        SM.Movement.StopHorizontal();
        SM.InputHandler.CancelBuffer();

        SM.AnimController.OnMotionEnd += HandleMotionEnd;
        SM.AnimController.PlaySpecial();
    }

    public override void Exit()
    {
        SM.AnimController.OnMotionEnd -= HandleMotionEnd;
    }

    public override void Update() { } // 特殊中は一切の入力を受け付けない

    private void HandleMotionEnd()
    {
        SM.TransitionTo(SM.Idle);
    }
}

public class DodgeState : PlayerState
{
    private enum Phase { PreLag, Active, PostLag }

    private Phase   _phase;
    private float   _phaseTimer;
    private Vector2 _dodgeInput;
    private bool    _isSpotDodge;
    private int     _penaltyLevel;
    private float   _lagMultiplier = 1f;
    private float   _activeDuration;
    private bool    _forceSpotDodge;

    public DodgeState(PlayerStateMachine sm) : base(sm) {}

    public void RequestSpotDodge() => _forceSpotDodge = true;

    public override void Enter()
    {
        _penaltyLevel = SM.RegisterDodgeUse();
        _lagMultiplier = SM.GetDodgeLagMultiplier(_penaltyLevel);
        _activeDuration = SM.DodgeConfig.ActiveDuration * _lagMultiplier;
        _isSpotDodge = _forceSpotDodge;
        _forceSpotDodge = false;
        _dodgeInput = _isSpotDodge ? Vector2.zero : SM.InputHandler.MoveInput;
        SM.AnimController.PlayDodge();
        SM.AnimController.SetPlaybackSpeed(1f / _lagMultiplier);
        EnterPhase(Phase.PreLag);
    }

    public override void Exit()
    {
        SM.AnimController.SetPlaybackSpeed(1f);
        SM.Movement.StopDodge();
        SM.DodgeCooldownTimer = SM.DodgeConfig.Cooldown;
    }

    public override void Update()
    {
        _phaseTimer -= Time.deltaTime;
        if (_phaseTimer > 0f) return;

        switch (_phase)
        {
            case Phase.PreLag:   EnterPhase(Phase.Active);  break;
            case Phase.Active:   SM.Movement.StopDodge();
                                 EnterPhase(Phase.PostLag); break;
            case Phase.PostLag:  SM.TransitionTo(SM.Idle);  break;
        }
    }

    private void EnterPhase(Phase next)
    {
        _phase = next;
        switch (next)
        {
            case Phase.PreLag:
                _phaseTimer = SM.DodgeConfig.PreLag * _lagMultiplier;
                SM.Movement.StopHorizontal();
                break;
            case Phase.Active:
                _phaseTimer = _activeDuration;
                SM.CharStats?.SetTimedHitReactionState(
                    HitReactionState.Invincible,
                    SM.GetDodgeInvincibleDuration(_penaltyLevel));
                SM.Movement.StartDodgeMove(_dodgeInput, _isSpotDodge, _activeDuration);
                break;
            case Phase.PostLag:
                _phaseTimer = SM.DodgeConfig.PostLag * _lagMultiplier;
                break;
        }
    }
}

public class AttackState : PlayerState
{
    private int  _comboIndex;
    private bool _cancellable;   // キャンセル行動が許可されているか

    public AttackState(PlayerStateMachine sm) : base(sm) {}

    public void SetComboIndex(int index) => _comboIndex = index;

    public override void Enter()
    {
        _cancellable = false;

        SM.Movement.StopHorizontal();
        SM.AnimController.OnBufferOpen       += HandleBufferOpen;
        SM.AnimController.OnCancellableFrame += HandleCancellableFrame;
        SM.AnimController.OnMotionEnd        += HandleMotionEnd;
        SM.AnimController.PlayAttack(_comboIndex);
        SM.Movement.FaceTarget();
    }

    public override void Exit()
    {
        SM.AnimController.OnBufferOpen       -= HandleBufferOpen;
        SM.AnimController.OnCancellableFrame -= HandleCancellableFrame;
        SM.AnimController.OnMotionEnd        -= HandleMotionEnd;
        SM.InputHandler.CloseAndCancelBuffer();
    }

    public override void Update()
    {
        if (!_cancellable) return;

        if (SM.InputHandler.AttackPressed || SM.InputHandler.ConsumeBufferedAttack())
        {
            _comboIndex = (_comboIndex + 1) % SM.AnimController.AttackCount;
            SM.TransitionTo(SM.Attack);
            return;
        }
        if (SM.InputHandler.HeavyAttackPressed)
        {
            SM.TransitionTo(SM.HeavyAttack);
            return;
        }

        if (SM.InputHandler.SpecialPressed)
        {
            SM.InputHandler.CancelBuffer();
            SM.TransitionTo(SM.Special);
            return;
        }

        if (SM.InputHandler.IsMoving)
        {
            SM.TransitionTo(SM.Idle);
            return;
        }

        if (SM.InputHandler.IsGuardHeld)
        {
            SM.TransitionTo(SM.Guard);
            return;
        }
    }

    private void HandleBufferOpen()
    {
        SM.InputHandler.OpenBuffer();
    }

    private void HandleCancellableFrame() => _cancellable = true;

    private void HandleMotionEnd()
    {
        SM.TransitionTo(SM.Idle);
    }
}

public class HeavyAttackState : PlayerState
{
    private bool _cancellable;

    public HeavyAttackState(PlayerStateMachine sm) : base(sm) { }

    public override void Enter()
    {
        _cancellable = false;
        SM.InputHandler.AttackLock = true;

        SM.Movement.StopHorizontal();
        SM.InputHandler.CloseAndCancelBuffer(); // 強攻撃はバッファを引き継がない
        SM.AnimController.OnCancellableFrame += HandleCancellableFrame;
        SM.AnimController.OnMotionEnd += HandleMotionEnd;
        SM.AnimController.PlayHeavyAttack();
    }

    public override void Exit()
    {
        SM.AnimController.OnCancellableFrame -= HandleCancellableFrame;
        SM.AnimController.OnMotionEnd -= HandleMotionEnd;
    }

    public override void Update()
    {
        if (!_cancellable) return;

        if (SM.InputHandler.IsMoving)
        {
            SM.TransitionTo(SM.Idle);
            return;
        }

        if (SM.InputHandler.IsGuardHeld)
        {
            SM.TransitionTo(SM.Guard);
            return;
        }

        if (SM.InputHandler.SpecialPressed)
        {
            SM.TransitionTo(SM.Special);
            return;
        }
    }

    private void HandleCancellableFrame() => _cancellable = true;
    private void HandleMotionEnd() => SM.TransitionTo(SM.Idle);
}

public class PlayerDeathState : PlayerState
{
    public PlayerDeathState(PlayerStateMachine sm) : base(sm) {}

    public override void Enter()
    {
        SM.Movement.StopHorizontal();
        SM.InputHandler.CancelBuffer();

        SM.AnimController.OnMotionEnd += HandleMotionEnd;
        SM.AnimController.PlayDeath();
    }

    public override void Exit()
    {
        SM.AnimController.OnMotionEnd -= HandleMotionEnd;
    }

    public override void Update() {} // 死亡中は一切の入力を受け付けない

    private void HandleMotionEnd()
    {
        SM.gameObject.SetActive(false);
    }
}

public class PlayerKnockbackState : PlayerState
{
    private Vector3 _dir;
    private float _distance;
    private float _duration;

    public PlayerKnockbackState(PlayerStateMachine sm) : base(sm) { }

    public void SetKnockback(Vector3 dir, float distance, float duration)
    {
        _dir = dir;
        _distance = distance;
        _duration = duration;
    }

    public override void Enter()
    {
        SM.Movement.StartKnockback(_dir, _distance, _duration);
        SM.AnimController.OnMotionEnd += HandleMotionEnd;
        SM.AnimController.PlayStagger();
    }

    public override void Exit()
    {
        SM.AnimController.OnMotionEnd -= HandleMotionEnd;
        SM.Movement.StopKnockback();
    }

    public override void Update() { } // ノックバック中は入力を受け付けない

    private void HandleMotionEnd() => SM.TransitionTo(SM.Idle);
}
