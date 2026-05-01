using UnityEngine;

/// <summary>
/// ヒット時にノックバックを発生させる IHitEffect 実装。
///
/// ■ ノックバックの方向
///   攻撃者から見て被攻撃者の逆方向（攻撃者 → 被攻撃者の方向）。
///   Y 成分は除いて水平方向のみに適用する。
///
/// ■ 移動方法
///   距離 ÷ 時間 で一定速度を計算し、EnemyMovement.StartKnockback() に渡す。
///
/// ■ モーション
///   EnemyStateMachine.TriggerKnockback() 経由で EnemyKnockbackState に遷移し、
///   Stagger モーションを流用する。
/// </summary>
[System.Serializable]
public class KnockbackHitEffect : IHitEffect
{
    public KnockbackSettings Settings = new KnockbackSettings();

    public void Execute(Collider hitCollider, Transform attacker)
    {
        if (Settings == null) return;

        // ノックバック方向：攻撃者から被攻撃者への水平方向
        Vector3 dir = hitCollider.transform.root.position - attacker.position;
        dir.y = 0f;

        // 方向がほぼゼロ（重なっている）なら攻撃者の前方を使う
        if (dir.sqrMagnitude < 0.001f)
            dir = attacker.forward;

        dir.Normalize();

        // 敵の EnemyStateMachine を取得してノックバック発動
        var enemySM = hitCollider.GetComponentInParent<EnemyStateMachine>();
        if (enemySM != null)
        {
            enemySM.TriggerKnockback(dir, Settings.Distance, Settings.Duration);
            return;
        }

        // プレイヤーへのノックバックは将来の拡張用に口だけ残す
        // var playerSM = hitCollider.GetComponentInParent<PlayerStateMachine>();
        // if (playerSM != null) { ... }
    }
}
