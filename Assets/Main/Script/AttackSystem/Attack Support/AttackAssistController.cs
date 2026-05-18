using UnityEngine;

/// <summary>
/// 攻撃アシストトラック（AttackAssistTrack）にバインドするコンポーネント。
/// PlayerMovement・LockOnController への参照を保持し、
/// Behaviour から呼ばれる移動操作 API を提供する。
/// </summary>
public class AttackAssistController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private LockOnController _lockOnController;

    [Header("Enemy Detection")]
    [Tooltip("ロックオンなし時に敵を探すレイヤー")]
    [SerializeField] private LayerMask _enemyDetectionLayer = 1 << 8;

    // --- プロパティ ---
    public PlayerMovement  PlayerMovement       => _playerMovement;
    public LockOnController LockOnController    => _lockOnController;
    public LayerMask        EnemyDetectionLayer => _enemyDetectionLayer;

    private Rigidbody _rb;

    private void Awake()
    {
        if (_playerMovement  == null) _playerMovement  = GetComponent<PlayerMovement>();
        if (_lockOnController == null) _lockOnController = GetComponent<LockOnController>();
        _rb = GetComponent<Rigidbody>();
    }

    // --- Behaviour から呼ばれる API ---

    /// <summary>スムーズ移動モード用：水平速度を上書きする。</summary>
    public void SetAssistVelocity(Vector3 velocity) => _playerMovement?.SetAssistVelocity(velocity);

    /// <summary>クリップ終了・移動完了時に水平速度を停止する。</summary>
    public void StopAssist() => _playerMovement?.StopHorizontal();

    /// <summary>ワープモード用：Rigidbody.MovePosition で瞬時に位置を変更する。</summary>
    public void WarpTo(Vector3 position)
    {
        if (_rb != null)
            _rb.MovePosition(position);
        else
            transform.position = position;
    }
}
