using UnityEngine;

/// <summary>
/// 現在装備している武器データを保持するコンポーネント。
/// プレイヤーの親 GameObject にアタッチする。
///
/// DamageHitEffect がこのコンポーネントから攻撃力・属性を取得する。
/// </summary>
public class WeaponHolder : MonoBehaviour
{
    [Header("現在の武器")]
    [SerializeField] private WeaponData _currentWeapon;

    public WeaponData CurrentWeapon => _currentWeapon;

    /// <summary>武器を切り替える（将来の拡張用）</summary>
    public void SetWeapon(WeaponData weapon) => _currentWeapon = weapon;
}
