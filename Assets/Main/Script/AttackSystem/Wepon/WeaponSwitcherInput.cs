using UnityEngine;

[RequireComponent(typeof(WeaponHolder))]
public class WeaponSwitcherInput : MonoBehaviour
{
    [SerializeField] private bool _useNumberKeys = true;
    [SerializeField] private bool _useMouseWheel = true;

    private WeaponHolder _weaponHolder;

    private void Awake()
    {
        _weaponHolder = GetComponent<WeaponHolder>();
    }

    private void Update()
    {
        if (_useNumberKeys)
            HandleNumberKeys();

        if (_useMouseWheel)
            HandleMouseWheel();
    }

    private void HandleNumberKeys()
    {
        WeaponData[] weapons = _weaponHolder.Weapons;
        if (weapons == null) return;

        int max = Mathf.Min(weapons.Length, 9);
        for (int i = 0; i < max; i++)
        {
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i)))
            {
                _weaponHolder.EquipWeapon(i);
                return;
            }
        }
    }

    private void HandleMouseWheel()
    {
        float wheel = Input.mouseScrollDelta.y;
        if (Mathf.Abs(wheel) <= 0.01f) return;

        if (wheel > 0f)
            _weaponHolder.EquipPrevious();
        else
            _weaponHolder.EquipNext();
    }
}
