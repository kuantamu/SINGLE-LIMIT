using UnityEngine;



#region 待機 / 移動ステート。
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
#endregion

#region 防御ステート
public class GuardState : PlayerState
{
    public GuardState(PlayerStateMachine sm) : base(sm) {}

    public override void Enter()
    {
        SM.Movement.StopHorizontal();
        SM.AnimController.PlayGuard();

        // 防御中フラグを立てる（ダメージ軽減に使用）
        var stats = SM.GetComponent<CharacterStats>();
        if (stats != null) stats.IsGuarding = true;
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
            var stats = SM.GetComponent<CharacterStats>();
            if (stats != null) stats.IsGuarding = false;
            SM.TransitionTo(SM.Idle);
            return;
        }

        // カメラ前方を向く
        SM.Movement.FaceCamera();

        if (SM.InputHandler.IsMoving && !SM.IsDodgeOnCooldown)
        {
            SM.TransitionTo(SM.Dodge);
        }
    }
}
#endregion

#region 特殊ステート
public class SpecialState : PlayerState
{
    public SpecialState(PlayerStateMachine sm) : base(sm){}

    public override void Enter()
    {
        // 入力・移動を完全に止める
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
        // モーション終了後に GameObject を無効化
        SM.TransitionTo(SM.Idle);
    }
}
#endregion

#region 回避ステート
public class DodgeState : PlayerState
{
    private enum Phase { PreLag, Active, PostLag }

    private Phase   _phase;
    private float   _phaseTimer;
    private Vector2 _dodgeInput;

    public DodgeState(PlayerStateMachine sm) : base(sm) {}

    public override void Enter()
    {
        _dodgeInput = SM.InputHandler.MoveInput;
        SM.AnimController.PlayDodge();
        EnterPhase(Phase.PreLag);
    }

    public override void Exit()
    {
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
                _phaseTimer = SM.DodgeConfig.PreLag;
                SM.Movement.StopHorizontal();
                break;
            case Phase.Active:
                _phaseTimer = SM.DodgeConfig.ActiveDuration;
                SM.Movement.StartDodgeMove(_dodgeInput);
                break;
            case Phase.PostLag:
                _phaseTimer = SM.DodgeConfig.PostLag;
                break;
        }
    }
}
#endregion

#region 攻撃ステート
public class AttackState : PlayerState
{
    private int  _comboIndex;
    private bool _bufferOpen;    // バッファ積み込みが許可されているか
    private bool _cancellable;   // キャンセル行動が許可されているか

    public AttackState(PlayerStateMachine sm) : base(sm) {}

    public void SetComboIndex(int index) => _comboIndex = index;

    public override void Enter()
    {
        _bufferOpen  = false;
        _cancellable = false;

        SM.Movement.StopHorizontal();
        SM.AnimController.OnBufferOpen       += HandleBufferOpen;
        SM.AnimController.OnCancellableFrame += HandleCancellableFrame;
        SM.AnimController.OnMotionEnd        += HandleMotionEnd;

        SM.AnimController.PlayAttack(_comboIndex);
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
        // キャンセル可能フレーム前は入力を無視
        if (!_cancellable) return;

        // コンボ継続（バッファ消費 or 直押し）
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

        // 移動キャンセル → Idle
        if (SM.InputHandler.IsMoving)
        {
            SM.TransitionTo(SM.Idle);
            return;
        }

        // 防御キャンセル → Guard
        if (SM.InputHandler.IsGuardHeld)
        {
            SM.TransitionTo(SM.Guard);
            return;
        }
    }

    // ---- Signal ハンドラ ----

    private void HandleBufferOpen()
    {
        _bufferOpen = true;
        SM.InputHandler.OpenBuffer(); // ここからバッファへの積み込みを許可
    }

    private void HandleCancellableFrame() => _cancellable = true;

    private void HandleMotionEnd()
    {
        SM.TransitionTo(SM.Idle);
    }
}
#endregion

#region 強攻撃ステート
/// <summary>
/// 強攻撃ステート（左クリック長押しで発動）。
/// 通常攻撃と同じ構造だが PlayHeavyAttack() を呼ぶ。
/// コンボからの強攻撃への派生も受け付ける。
/// </summary>
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
#endregion

#region プレイヤー死亡ステート
public class PlayerDeathState : PlayerState
{
    public PlayerDeathState(PlayerStateMachine sm) : base(sm) {}

    public override void Enter()
    {
        // 入力・移動を完全に止める
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
        // モーション終了後に GameObject を無効化
        SM.gameObject.SetActive(false);
    }
}
#endregion