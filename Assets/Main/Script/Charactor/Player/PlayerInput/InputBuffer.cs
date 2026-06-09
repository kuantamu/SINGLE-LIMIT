public class InputBuffer
{
    private const float BufferDuration = 0.2f;
    private float _timer;

    public bool HasAttack => _timer > 0f;

    public void Tick(float deltaTime)
    {
        if (_timer > 0f) _timer -= deltaTime;
    }

    public void SetAttack() => _timer = BufferDuration;

    public bool ConsumeAttack()
    {
        if (!HasAttack) return false;
        _timer = 0f;
        return true;
    }

    public void Cancel() => _timer = 0f;
}
