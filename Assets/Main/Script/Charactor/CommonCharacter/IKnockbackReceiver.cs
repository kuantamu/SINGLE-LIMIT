using UnityEngine;

public interface IKnockbackReceiver
{
    void TriggerKnockback(Vector3 dir, float distance, float duration);
}
