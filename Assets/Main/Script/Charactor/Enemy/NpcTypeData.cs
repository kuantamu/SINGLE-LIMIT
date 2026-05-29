using UnityEngine;

[CreateAssetMenu(fileName = "NpcTypeData", menuName = "NPC/Npc Type")]
public class NpcTypeData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string _displayName = "NPC";
    [SerializeField] private GameObject _prefab;
    [SerializeField, Min(0f)] private float _spawnWeight = 1f;

    [Header("Stats")]
    [SerializeField] private CharacterStatData _statData;

    [Header("Movement")]
    [SerializeField, Min(0f)] private float _moveSpeed = 3f;
    [SerializeField, Min(0f)] private float _acceleration = 15f;
    [SerializeField, Min(0f)] private float _rotationSpeed = 360f;

    [Header("Detection")]
    [SerializeField, Min(0f)] private float _detectionRange = 10f;
    [SerializeField, Range(0f, 180f)] private float _fieldOfView = 60f;
    [SerializeField] private LayerMask _detectionLayer;
    [SerializeField] private LayerMask _obstacleLayer;
    [SerializeField] private bool _useLineOfSight = true;
    [SerializeField, Min(0.01f)] private float _detectionUpdateInterval = 0.1f;

    [Header("Combat")]
    [SerializeField, Min(0f)] private float _attackRange = 2f;
    [SerializeField, Min(0f)] private float _attackInterval = 2f;

    public string DisplayName => _displayName;
    public GameObject Prefab => _prefab;
    public float SpawnWeight => _spawnWeight;
    public CharacterStatData StatData => _statData;
    public float MoveSpeed => _moveSpeed;
    public float Acceleration => _acceleration;
    public float RotationSpeed => _rotationSpeed;
    public float DetectionRange => _detectionRange;
    public float FieldOfView => _fieldOfView;
    public LayerMask DetectionLayer => _detectionLayer;
    public LayerMask ObstacleLayer => _obstacleLayer;
    public bool UseLineOfSight => _useLineOfSight;
    public float DetectionUpdateInterval => _detectionUpdateInterval;
    public float AttackRange => _attackRange;
    public float AttackInterval => _attackInterval;
}
