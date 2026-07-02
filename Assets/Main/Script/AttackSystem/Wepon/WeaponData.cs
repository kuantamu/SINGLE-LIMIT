using UnityEngine;
using UnityEngine.Timeline;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Stats/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string DisplayName;

    [Header("Weapon Prefabs")]
    public GameObject MainHandPrefab;
    public GameObject OffHandPrefab;

    [Header("Combat Parameters")]
    public int AttackPower = 15;
    public AttributeType Attribute = AttributeType.Slash;
    [Range(0f, 1f)] public float CriticalRateBonus;
    public float CriticalMultiplierBonus;
    public float DamageMultiplier = 1f;

    [Header("State Settings")]
    public WeaponStateSettings States = new WeaponStateSettings();

    [Header("Animation Timelines")]
    public WeaponAnimationSet Animations = new WeaponAnimationSet();
}

[System.Serializable]
public class WeaponStateSettings
{
    public bool CanAttack = true;
    public bool CanHeavyAttack = true;
    public bool CanGuard = true;
    public bool CanSpecial = true;
    public bool CanDodge = true;
}

[System.Serializable]
public class WeaponAnimationSet
{
    public TimelineAsset Idle;
    public TimelineAsset Move;
    public TimelineAsset Guard;
    public TimelineAsset Dodge;
    public TimelineAsset Death;
    public TimelineAsset Special;
    public TimelineAsset Stagger;
    public TimelineAsset[] Attacks;
    public TimelineAsset HeavyAttack;
}
