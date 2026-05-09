/// <summary>
/// 攻撃入力を一定時間保持するバッファ。
/// 他の行動に移行した場合は Cancel() を呼んで即クリアする。
/// </summary>
public class InputBuffer
{
    // アクションゲームの標準的なバッファ時間は 0.1~0.3秒
    // 0.2秒なら約 12フレーム分（60FPS 想定）
    private const float BufferDuration = 0.2f;
    private float _timer;

    /// <summary>バッファに攻撃入力が残っているか</summary>
    public bool HasAttack => _timer > 0f;

    /// <summary>毎フレーム呼ぶ。タイマーを減算する。</summary>
    public void Tick(float deltaTime)
    {
        if (_timer > 0f) _timer -= deltaTime;
    }

    /// <summary>攻撃ボタンが押された時に呼ぶ。</summary>
    public void SetAttack() => _timer = BufferDuration;

    /// <summary>バッファを消費する。入力があれば true を返しクリアする。</summary>
    public bool ConsumeAttack()
    {
        if (!HasAttack) return false;
        _timer = 0f;
        return true;
    }

    /// <summary>他の行動に移行した時に呼ぶ。即クリアする。</summary>
    public void Cancel() => _timer = 0f;
}
