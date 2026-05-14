public abstract class PlayerState : ICharacterState
{
    protected readonly PlayerStateMachine SM;

    protected PlayerState(PlayerStateMachine sm) { SM = sm; }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
}
