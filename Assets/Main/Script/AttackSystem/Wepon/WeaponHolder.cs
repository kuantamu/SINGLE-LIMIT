using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    [Header("現在の武器")]
    [SerializeField] private WeaponData _currentWeapon;

    public WeaponData CurrentWeapon => _currentWeapon;

    public void SetWeapon(WeaponData weapon) => _currentWeapon = weapon;
}
