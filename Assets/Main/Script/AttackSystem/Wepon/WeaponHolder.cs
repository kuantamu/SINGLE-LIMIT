using System;
using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    [Header("Weapon Data")]
    [SerializeField] private WeaponData[] _weapons;
    [SerializeField] private WeaponData _currentWeapon;
    [SerializeField] private int _initialWeaponIndex;

    [Header("Equip Points")]
    [SerializeField] private Transform _mainHandSocket;
    [SerializeField] private Transform _offHandSocket;

    private GameObject _mainHandInstance;
    private GameObject _offHandInstance;
    private int _currentIndex = -1;

    public event Action<WeaponData> OnWeaponChanged;

    public WeaponData CurrentWeapon => _currentWeapon;
    public WeaponData[] Weapons => _weapons;
    public int CurrentIndex => _currentIndex;

    private void Start()
    {
        if (_weapons != null && _weapons.Length > 0)
        {
            int index = Mathf.Clamp(_initialWeaponIndex, 0, _weapons.Length - 1);
            EquipWeapon(index);
            return;
        }

        ApplyWeapon(_currentWeapon);
    }

    public void SetWeapon(WeaponData weapon) => EquipWeapon(weapon);

    public bool EquipWeapon(WeaponData weapon)
    {
        if (weapon == null) return false;

        int index = FindWeaponIndex(weapon);
        _currentIndex = index;
        _currentWeapon = weapon;
        ApplyWeapon(_currentWeapon);
        return true;
    }

    public bool EquipWeapon(int index)
    {
        if (_weapons == null || index < 0 || index >= _weapons.Length) return false;
        return EquipWeapon(_weapons[index]);
    }

    public bool EquipNext()
    {
        if (_weapons == null || _weapons.Length == 0) return false;

        int next = _currentIndex < 0 ? 0 : (_currentIndex + 1) % _weapons.Length;
        return EquipWeapon(next);
    }

    public bool EquipPrevious()
    {
        if (_weapons == null || _weapons.Length == 0) return false;

        int previous = _currentIndex < 0
            ? 0
            : (_currentIndex - 1 + _weapons.Length) % _weapons.Length;
        return EquipWeapon(previous);
    }

    private int FindWeaponIndex(WeaponData weapon)
    {
        if (_weapons == null) return -1;

        for (int i = 0; i < _weapons.Length; i++)
        {
            if (_weapons[i] == weapon)
                return i;
        }

        return -1;
    }

    private void ApplyWeapon(WeaponData weapon)
    {
        ClearWeaponInstances();

        if (weapon != null)
        {
            _mainHandInstance = CreateWeaponInstance(weapon.MainHandPrefab, _mainHandSocket);
            _offHandInstance = CreateWeaponInstance(weapon.OffHandPrefab, _offHandSocket);
        }

        OnWeaponChanged?.Invoke(weapon);
    }

    private GameObject CreateWeaponInstance(GameObject prefab, Transform socket)
    {
        if (prefab == null || socket == null) return null;

        GameObject instance = Instantiate(prefab, socket);
        Transform instanceTransform = instance.transform;
        instanceTransform.localPosition = Vector3.zero;
        instanceTransform.localRotation = Quaternion.identity;
        instanceTransform.localScale = Vector3.one;
        return instance;
    }

    private void ClearWeaponInstances()
    {
        DestroyWeaponInstance(_mainHandInstance);
        DestroyWeaponInstance(_offHandInstance);
        _mainHandInstance = null;
        _offHandInstance = null;
    }

    private void DestroyWeaponInstance(GameObject instance)
    {
        if (instance == null) return;

        if (Application.isPlaying)
            Destroy(instance);
        else
            DestroyImmediate(instance);
    }
}
