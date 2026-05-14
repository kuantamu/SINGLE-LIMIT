public abstract class EnemyState : ICharacterState
{
    protected readonly EnemyStateMachine SM;

    protected EnemyState(EnemyStateMachine sm) { SM = sm; }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
}
