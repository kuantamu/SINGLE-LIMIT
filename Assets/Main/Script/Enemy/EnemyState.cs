/// <summary>
/// 全敵ステートの抽象基底クラス。
/// </summary>
public abstract class EnemyState
{
    protected readonly EnemyStateMachine SM;

    protected EnemyState(EnemyStateMachine sm) { SM = sm; }

    public virtual void Enter()  {}
    public virtual void Exit()   {}
    public virtual void Update() {}
}
