using UnityEngine;

[System.Serializable]
public class KnockbackHitEffect : IHitEffect
{
    public KnockbackSettings Settings = new KnockbackSettings();

    public void Execute(Collider hitCollider, Transform attacker)
    {
        if (Settings == null) return;

        Vector3 dir = hitCollider.transform.root.position - attacker.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
            dir = attacker.forward;

        dir.Normalize();

        IKnockbackReceiver receiver = hitCollider.GetComponentInParent<IKnockbackReceiver>();
        receiver?.TriggerKnockback(dir, Settings.Distance, Settings.Duration);
    }
}
