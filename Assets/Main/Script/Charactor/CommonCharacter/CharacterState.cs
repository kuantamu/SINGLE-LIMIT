/// <summary>
/// キャラクターの基底ステート
/// </summary>

public interface ICharacterState
{
    void Enter();
    void Exit();
    void Update();
}
