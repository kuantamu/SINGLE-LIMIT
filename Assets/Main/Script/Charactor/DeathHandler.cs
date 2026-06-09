using UnityEngine;

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
        var stateMachine = GetComponent<CharacterStateMachineBase>();
        if(stateMachine != null)
        {
            stateMachine.TriggerDeath();
            return;
        }

        Debug.LogWarning($"[DeathHandler] {gameObject.name} に PlayerStateMachine も EnemyStateMachine も見つかりませんでした。");
    }
}
