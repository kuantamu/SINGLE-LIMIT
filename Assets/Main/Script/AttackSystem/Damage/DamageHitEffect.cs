using UnityEngine;

[System.Serializable]
public class DamageHitEffect : IHitEffect
{
    public float DamageEffectSkillPower;
    
    public void Execute(Collider hitCollider, Transform attacker)
    {
        var defenderStats = hitCollider.GetComponentInParent<CharacterStats>();
        if (defenderStats == null) return;

        var weaponHolder = attacker.GetComponentInParent<WeaponHolder>();
        if (weaponHolder == null || weaponHolder.CurrentWeapon == null)
        {
            Debug.LogWarning("[DamageHitEffect] WeaponHolder または WeaponData が設定されていません。");
            return;
        }

        var weapon        = weaponHolder.CurrentWeapon;
        var attackerStats = attacker.GetComponentInParent<CharacterStats>();

        var info = new DamageInfo
        {
            AttackPower = weapon.AttackPower,
            Attribute = weapon.Attribute,
            CriticalRate = attackerStats != null ? attackerStats.StatData.CriticalRate : 0f,
            CriticalMultiplier = attackerStats != null ? attackerStats.StatData.CriticalMultiplier : 1.5f,
            IsGuarded = false, // CharacterStats.TakeDamage 内で上書きされる
            SkillPower = DamageEffectSkillPower,
            OutgoingDamageMultiplier = attackerStats != null ? attackerStats.OutgoingDamageMultiplier : 1f,
            UseOutgoingDamageMultiplier = true,
            IncomingDamageMultiplier = 1f,
            AttackChara = attackerStats
        };

        defenderStats.TakeDamage(info);
    }
}
