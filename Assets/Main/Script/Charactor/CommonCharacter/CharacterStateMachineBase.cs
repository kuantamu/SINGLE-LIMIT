using UnityEngine;
/// <summary>
/// キャラクターの基礎ステートマシーン
/// </summary>
public abstract class CharacterStateMachineBase : MonoBehaviour, IKnockbackReceiver
{
    protected CharacterStats Stats { get; private set; }
    public ICharacterState CurrentState { get; private set; }

    protected virtual void Awake()
    {
        Stats = GetComponent<CharacterStats>();
        InitializeStateMachine();
    }

    //HPが０となった時に死亡するイベントを購読
    protected virtual void Start()
    {
        if (Stats != null)
            Stats.OnDeath += TriggerDeath;

        TransitionTo(GetInitialState());
    }

    protected virtual void OnDestroy()
    {
        if (Stats != null)
            Stats.OnDeath -= TriggerDeath;
    }

    protected virtual void Update()
    {
        OnBeforeStateUpdate();
        CurrentState?.Update();
    }

    protected virtual void OnBeforeStateUpdate() { }
    //ステート登録
    protected abstract void InitializeStateMachine();
    //最初のステート
    protected abstract ICharacterState GetInitialState();

    //死亡するステート
    protected abstract ICharacterState GetDeathState();

    //ステートを呼び出す。
    public void TransitionTo(ICharacterState next)
    {
        if (next == null) return;

        CurrentState?.Exit();
        CurrentState = next;
        CurrentState.Enter();
    }

    //
    public virtual void TriggerDeath()
    {
        TransitionTo(GetDeathState());
    }

    protected bool IsDeadState => CurrentState == GetDeathState();

    public abstract void TriggerKnockback(Vector3 dir, float distance, float duration);
}
