using UnityEngine;

/// <summary>
/// ヒットした対象にダメージを与える効果。
///
/// ■ 必要なもの
///   攻撃者の親に WeaponHolder コンポーネントをアタッチして
///   現在の WeaponData を設定しておくこと。
///   被攻撃者には CharacterStats コンポーネントが必要。
/// </summary>
[System.Serializable]
public class DamageHitEffect : IHitEffect
{
    public float DamageEffectSkillPower;
    
    public void Execute(Collider hitCollider, Transform attacker)
    {
        // 被攻撃者の CharacterStats を取得
        var defenderStats = hitCollider.GetComponentInParent<CharacterStats>();
        if (defenderStats == null) return;

        // 攻撃者の WeaponHolder から武器データを取得
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
            AttackPower         = weapon.AttackPower,
            Attribute           = weapon.Attribute,
            CriticalRate        = attackerStats != null ? attackerStats.StatData.CriticalRate : 0f,
            CriticalMultiplier  = attackerStats != null ? attackerStats.StatData.CriticalMultiplier : 1.5f,
            IsGuarded           = false, // CharacterStats.TakeDamage 内で上書きされる
            SkillPower          = DamageEffectSkillPower
        };

        defenderStats.TakeDamage(info);
    }
}
