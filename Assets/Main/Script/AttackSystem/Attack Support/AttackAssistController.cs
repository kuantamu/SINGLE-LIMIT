using UnityEngine;

public class AttackAssistController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private LockOnController _lockOnController;

    [Header("Enemy Detection")]
    [Tooltip("ロックオンなし時に敵を探すレイヤー")]
    [SerializeField] private LayerMask _enemyDetectionLayer = 1 << 8;

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

    public void SetAssistVelocity(Vector3 velocity) => _playerMovement?.SetAssistVelocity(velocity);

    public void StopAssist() => _playerMovement?.StopHorizontal();

    public void WarpTo(Vector3 position)
    {
        if (_rb != null)
            _rb.MovePosition(position);
        else
            transform.position = position;
    }
}
