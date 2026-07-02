using UnityEngine;

[System.Serializable]
public class DamageHitEffect : IHitEffect
{
    public float DamageEffectSkillPower;

    public void Execute(Collider hitCollider, Transform attacker)
    {
        CharacterStats defenderStats = hitCollider.GetComponentInParent<CharacterStats>();
        if (defenderStats == null) return;

        WeaponHolder weaponHolder = attacker.GetComponentInParent<WeaponHolder>();
        if (weaponHolder == null || weaponHolder.CurrentWeapon == null)
        {
            Debug.LogWarning("[DamageHitEffect] WeaponHolder or WeaponData is not configured.");
            return;
        }

        WeaponData weapon = weaponHolder.CurrentWeapon;
        CharacterStats attackerStats = attacker.GetComponentInParent<CharacterStats>();
        float damageMultiplier = Mathf.Max(0f, weapon.DamageMultiplier);

        var info = new DamageInfo
        {
            AttackPower = Mathf.RoundToInt(weapon.AttackPower * damageMultiplier),
            Attribute = weapon.Attribute,
            CriticalRate = Mathf.Clamp01((attackerStats != null ? attackerStats.StatData.CriticalRate : 0f) + weapon.CriticalRateBonus),
            CriticalMultiplier = (attackerStats != null ? attackerStats.StatData.CriticalMultiplier : 1.5f) + weapon.CriticalMultiplierBonus,
            IsGuarded = false,
            SkillPower = DamageEffectSkillPower,
            OutgoingDamageMultiplier = attackerStats != null ? attackerStats.OutgoingDamageMultiplier : 1f,
            UseOutgoingDamageMultiplier = true,
            IncomingDamageMultiplier = 1f,
            AttackChara = attackerStats
        };

        defenderStats.TakeDamage(info);
    }
}
