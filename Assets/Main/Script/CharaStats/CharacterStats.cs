using System;
using System.Collections.Generic;
using UnityEngine;



//キャラクターがどのような状態か
public enum HitReactionState
{
    Normal,

    Down,

    Armor,

    Invincible,

    Dead
}

[RequireComponent(typeof(CharacterBlinkVisual))]
public class CharacterStats : MonoBehaviour
{
    [Header("Status Data")]
    [SerializeField] private CharacterStatData _statData;

    [Header("Faction")]
    [SerializeField] private CharacterFaction _faction = CharacterFaction.Other;

    [SerializeField] private bool _useDefaultFactionRelations = true;

    [SerializeField] private CharacterFactionMask _hostileFactions   = CharacterFactionMask.None;

    [SerializeField] private CharacterFactionMask _attackableFactions = CharacterFactionMask.None;

    [Header("Hit Reaction Debug")]
    [SerializeField] private HitReactionState _hitReactionState = HitReactionState.Normal;

    [SerializeField] public CharacterBlinkVisual _blinkVisual = new CharacterBlinkVisual();

    private readonly List<DamageBuff> _damageBuffs = new List<DamageBuff>();

    private HitReactionState _stateBeforeTimedState;

    private float _timedStateTimer;

    private bool _hasTimedState;

    public CharacterStats lastAttackChara;

    public CharacterStatData StatData => _statData;

    public CharacterFaction Faction => _faction;

    public CharacterFactionMask HostileFactions => _useDefaultFactionRelations
        ? CharacterFactionRules.DefaultHostileFactions(_faction)
        : _hostileFactions;

    public CharacterFactionMask AttackableFactions => _useDefaultFactionRelations
        ? CharacterFactionRules.DefaultAttackableFactions(_faction)
        : _attackableFactions;

    public CharacterFaction GetFaction() => _faction;

    public CharacterFactionMask GetEnemyFaction() => _hostileFactions;

    #region 現在のステータス関連
    public void SetStatData(CharacterStatData statData, bool resetHp)
    {
        if (statData == null) return;

        _statData = statData;
        if (resetHp)
            CurrentHP = _statData.MaxHP;
    }

    public int CurrentHP { get; private set; }

    public int MaxHP => _statData == null ? 0 : _statData.MaxHP;

    public bool IsDead => CurrentHP <= 0;

    public bool IsGuarding { get; set; }

    public HitReactionState CurrentHitReactionState => _hitReactionState;

    public bool IsInvincible => _hitReactionState == HitReactionState.Invincible;

    public bool IsArmor => _hitReactionState == HitReactionState.Armor;

    public bool IsDown => _hitReactionState == HitReactionState.Down;

    public bool CanReceiveHit => !IsDead && !IsInvincible;

    public bool CanBeKnockedBack => !IsDead && !IsInvincible && !IsArmor;

    public bool IsHostileTo(CharacterStats target) => CharacterFactionRules.IsHostile(this, target);

    public bool CanAttack(CharacterStats target) => CharacterFactionRules.CanAttack(this, target);

    public float OutgoingDamageMultiplier => GetDamageMultiplier(DamageBuffTarget.OutgoingDamage);

    public float IncomingDamageMultiplier => GetDamageMultiplier(DamageBuffTarget.IncomingDamage);

    #endregion

    public event Action<int, bool, AttributeType> OnDamaged;

    public event Action OnDeath;

    public event Action OnDamageBuffsChanged;

    private void Awake()
    {
        _blinkVisual.CacheRenderers(_hitReactionState);

        if (_statData == null)
        {
            Debug.LogWarning($"[CharacterStats] {gameObject.name} has no StatData.");
            return;
        }

        CurrentHP = _statData.MaxHP;
    }

    private void Update()
    {
        TickDamageBuffs(Time.deltaTime);

        TickTimedHitReactionState(Time.deltaTime);

        _blinkVisual.UpdateHitReactionVisual(IsInvincible,_hitReactionState);

        IsCharacterDead();
    }

    private void OnValidate()
    {
        if (!_useDefaultFactionRelations) return;

        _hostileFactions = CharacterFactionRules.DefaultHostileFactions(_faction);
        _attackableFactions = CharacterFactionRules.DefaultAttackableFactions(_faction);
    }
    //damageinfoから取ってくる
    public void TakeDamage(DamageInfo info)
    {
        lastAttackChara = info.AttackChara;
        if (IsDead || _statData == null || !CanReceiveHit) return;

        info.IsGuarded = IsGuarding;
        info.IncomingDamageMultiplier = IncomingDamageMultiplier;
        info.UseIncomingDamageMultiplier = true;

        info.ForceWeakAttribute = IsDown;

        int damage = DamageCalculator.Calculate(info, _statData, out bool isCritical);

        CurrentHP = Mathf.Max(0, CurrentHP - damage);

        OnDamaged?.Invoke(damage, isCritical, info.Attribute);

        if (CurrentHP <= 0)
            OnDeath?.Invoke();
    }

    //ヒット判定の状態をセットする
    public void SetHitReactionState(HitReactionState state)
    {
        _hasTimedState = false;
        _timedStateTimer = 0f;
        _hitReactionState = state;
    }
    //時間の条件付きヒット判定の状態を付ける
    public void SetHitReactionState(HitReactionState state, float duration)
    {
        if (duration <= 0f)
        {
            SetHitReactionState(state);
            return;
        }

        _stateBeforeTimedState = _hasTimedState ? _stateBeforeTimedState : _hitReactionState;
        _hitReactionState = state;
        _timedStateTimer = duration;
        _hasTimedState = true;
    }
    //属性耐性変更システム
    public ResistanceLevel GetEffectiveAttributeResistanceLevel(AttributeType attribute)
    {
        if (IsDown) return ResistanceLevel.Weak;
        return _statData == null ? ResistanceLevel.Normal : _statData.GetAttributeResistanceLevel(attribute);
    }
    public DamageBuff AddDamageBuff(
        DamageBuffTarget target,
        float multiplier,
        float duration          = -1f,
        string id               = null,
        bool overwriteMultiplier = true,
        bool overwriteDuration   = true)
    {
        DamageBuff existing = FindDamageBuff(id);
        if (existing != null)
        {
            existing.Refresh(multiplier, duration, overwriteMultiplier, overwriteDuration);
            OnDamageBuffsChanged?.Invoke();
            return existing;
        }

        DamageBuff buff = new DamageBuff(id, target, multiplier, duration);
        _damageBuffs.Add(buff);
        OnDamageBuffsChanged?.Invoke();
        return buff;
    }
    public bool RemoveDamageBuff(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;

        for (int i = _damageBuffs.Count - 1; i >= 0; i--)
        {
            if (_damageBuffs[i].Id != id) continue;

            _damageBuffs.RemoveAt(i);
            OnDamageBuffsChanged?.Invoke();
            return true;
        }

        return false;
    }
    public void ClearDamageBuffs()
    {
        if (_damageBuffs.Count == 0) return;

        _damageBuffs.Clear();
        OnDamageBuffsChanged?.Invoke();
    }
    private void TickDamageBuffs(float deltaTime)
    {
        if (_damageBuffs.Count == 0) return;

        bool changed = false;
        for (int i = _damageBuffs.Count - 1; i >= 0; i--)
        {
            _damageBuffs[i].Tick(deltaTime);
            if (!_damageBuffs[i].IsExpired) continue;

            _damageBuffs.RemoveAt(i);
            changed = true;
        }

        if (changed)
            OnDamageBuffsChanged?.Invoke();
    }
    private float GetDamageMultiplier(DamageBuffTarget target)
    {
        float multiplier = 1f;
        for (int i = 0; i < _damageBuffs.Count; i++)
        {
            DamageBuff buff = _damageBuffs[i];
            if (buff.Target != target) continue;

            multiplier *= buff.Multiplier;
        }

        return multiplier;
    }
    private DamageBuff FindDamageBuff(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        for (int i = 0; i < _damageBuffs.Count; i++)
        {
            if (_damageBuffs[i].Id == id)
                return _damageBuffs[i];
        }

        return null;
    }
    private void TickTimedHitReactionState(float deltaTime)
    {
        if (!_hasTimedState) return;

        _timedStateTimer -= deltaTime;
        if (_timedStateTimer > 0f) return;

        _hitReactionState = _stateBeforeTimedState;
        _timedStateTimer  = 0f;
        _hasTimedState = false;
    }
    
    
    void IsCharacterDead()
    {
        if(_hitReactionState == HitReactionState.Dead)
        {
            CurrentHP = 0;
            OnDeath?.Invoke();
        }
    }
}
