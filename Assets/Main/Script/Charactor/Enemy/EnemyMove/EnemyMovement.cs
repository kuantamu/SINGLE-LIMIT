using UnityEngine;

//CharacterMovementBaseを継承
public class EnemyMovement : CharacterMovementBase
{
    #region 変数
    [Header("Move")]
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _acceleration = 15f;

    [Header("Rotation")]
    [SerializeField] private float _rotationSpeed = 360f;
    #endregion

    //移動処理
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

    //向かう方向を向く処理
    public void FaceToward(Vector3 targetPos)
    {
        RotateToward(targetPos);
    }

    //向かう方向に向く処理
    private void RotateToward(Vector3 targetPos)
    {
        Vector3 dir = targetPos - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        FaceDirection(dir, _rotationSpeed);
    }
}
