using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class BuffDebuffApplication
{
    [FormerlySerializedAs("_target")]
    [SerializeField] private DamageBuffTarget _buffType = DamageBuffTarget.IncomingDamage;
    [SerializeField] private float _multiplier = 1f;
    [SerializeField] private float _duration = 5f;
    [SerializeField] private bool _overwriteMultiplier = true;
    [SerializeField] private bool _overwriteDuration = true;

    public DamageBuffTarget BuffType => _buffType;
    public float Multiplier => _multiplier;
    public float Duration => _duration;
    public bool OverwriteMultiplier => _overwriteMultiplier;
    public bool OverwriteDuration => _overwriteDuration;
    public string BuffKey => _buffType.ToString();
}
