/// <summary>
/// 全プレイヤーステートの抽象基底クラス。
/// </summary>
public abstract class PlayerState
{
    protected readonly PlayerStateMachine SM;

    protected PlayerState(PlayerStateMachine sm) { SM = sm; }

    public virtual void Enter()  {}
    public virtual void Exit()   {}
    public virtual void Update() {}
}
