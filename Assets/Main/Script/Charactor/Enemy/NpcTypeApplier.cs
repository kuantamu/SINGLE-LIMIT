using UnityEngine;

[DefaultExecutionOrder(-100)]
public class NpcTypeApplier : MonoBehaviour
{
    [SerializeField] private NpcTypeData _type;
    [SerializeField] private bool _applyOnAwake = true;

    public NpcTypeData Type => _type;

    private void Awake()
    {
        if (_applyOnAwake)
            Apply();
    }

    public void Apply(NpcTypeData type)
    {
        _type = type;
        Apply();
    }

    public void Apply()
    {
        if (_type == null) return;

        CharacterStats stats = GetComponent<CharacterStats>();
        if (stats != null)
            stats.SetStatData(_type.StatData, true);

        EnemyMovement movement = GetComponent<EnemyMovement>();
        if (movement != null)
            movement.ApplyNpcType(_type);

        EnemyDetector detector = GetComponent<EnemyDetector>();
        if (detector != null)
            detector.ApplyNpcType(_type);

        EnemyStateMachine stateMachine = GetComponent<EnemyStateMachine>();
        if (stateMachine != null)
            stateMachine.ApplyNpcType(_type);
    }
}
