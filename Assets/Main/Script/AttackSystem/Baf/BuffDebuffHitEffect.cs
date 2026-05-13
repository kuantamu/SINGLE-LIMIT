using System;
using UnityEngine;

[Serializable]
public class BuffDebuffHitEffect : IHitEffect
{
    public BuffDebuffApplication[] Applications;

    public void Execute(Collider hitCollider, Transform attacker)
    {
        CharacterStats targetStats = hitCollider.GetComponentInParent<CharacterStats>();
        if (targetStats == null || Applications == null) return;

        for (int i = 0; i < Applications.Length; i++)
        {
            BuffDebuffApplication application = Applications[i];
            if (application == null) continue;

            targetStats.AddDamageBuff(
                application.BuffType,
                application.Multiplier,
                application.Duration,
                application.BuffKey,
                application.OverwriteMultiplier,
                application.OverwriteDuration);
        }
    }
}
