using UnityEngine;

public class EnemyMovement : CharacterMovementBase
{
    [Header("Move")]
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _acceleration = 15f;

    [Header("Rotation")]
    [SerializeField] private float _rotationSpeed = 360f;

    public void ApplyNpcType(NpcTypeData type)
    {
        if (type == null) return;

        _moveSpeed = type.MoveSpeed;
        _acceleration = type.Acceleration;
        _rotationSpeed = type.RotationSpeed;
    }

    public void MoveToward(Vector3 targetPos)
    {
        Vector3 dir = targetPos - transform.position;
        dir.y = 0f;
        dir.Normalize();

        Vector3 target = dir * _moveSpeed;
        HorizontalVelocity = Vector3.MoveTowards(
            HorizontalVelocity, target, _acceleration * Time.deltaTime);

        RotateToward(targetPos);
    }

    public void FaceToward(Vector3 targetPos)
    {
        RotateToward(targetPos);
    }

    private void RotateToward(Vector3 targetPos)
    {
        Vector3 dir = targetPos - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        FaceDirection(dir, _rotationSpeed);
    }
}
