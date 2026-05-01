using UnityEngine;

/// <summary>
/// CharacterStats.OnDeath イベントを状態機械の TriggerDeath() に接続するコンポーネント。
/// プレイヤー・敵ともに同じ GameObject にアタッチする。
///
/// Awake() で CharacterStats.OnDeath を購読し、
/// 発火時にプレイヤーなら PlayerStateMachine.TriggerDeath()、
/// 敵なら EnemyStateMachine.TriggerDeath() を呼ぶ。
/// </summary>
[RequireComponent(typeof(CharacterStats))]
public class DeathHandler : MonoBehaviour
{
    private void Awake()
    {
        var stats = GetComponent<CharacterStats>();
        stats.OnDeath += HandleDeath;
    }

    private void HandleDeath()
    {
        // プレイヤーの場合
        var playerSM = GetComponent<PlayerStateMachine>();
        if (playerSM != null)
        {
            playerSM.TriggerDeath();
            return;
        }

        // 敵の場合
        var enemySM = GetComponent<EnemyStateMachine>();
        if (enemySM != null)
        {
            enemySM.TriggerDeath();
            return;
        }

        Debug.LogWarning($"[DeathHandler] {gameObject.name} に PlayerStateMachine も EnemyStateMachine も見つかりませんでした。");
    }
}
