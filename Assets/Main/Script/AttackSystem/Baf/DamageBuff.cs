using System;
using UnityEngine;

[Serializable]
public class DamageBuff
{
    [SerializeField] private string _id;
    [SerializeField] private DamageBuffTarget _target;
    [SerializeField] private float _multiplier = 1f;
    [SerializeField] private float _duration = -1f;

    private float _remainingTime;

    public string Id => _id;
    public DamageBuffTarget Target => _target;
    public float Multiplier => Mathf.Max(0f, _multiplier);
    public float Duration => _duration;
    public float RemainingTime => _remainingTime;
    public bool IsPermanent => _duration < 0f;
    public bool IsExpired => !IsPermanent && _remainingTime <= 0f;

    public DamageBuff(string id, DamageBuffTarget target, float multiplier, float duration = -1f)
    {
        _id = id;
        _target = target;
        _multiplier = multiplier;
        _duration = duration;
        _remainingTime = duration;
    }

    public void Refresh(
        float multiplier,
        float duration,
        bool overwriteMultiplier = true,
        bool overwriteDuration = true)
    {
        if (overwriteMultiplier)
            _multiplier = multiplier;

        if (!overwriteDuration) return;

        _duration = duration;
        _remainingTime = duration;
    }

    public void Tick(float deltaTime)
    {
        if (IsPermanent) return;
        _remainingTime -= deltaTime;
    }
}
