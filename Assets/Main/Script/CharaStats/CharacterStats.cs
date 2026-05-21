using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>被弾したときのキャラクター側の反応状態。</summary>
public enum HitReactionState
{
    Normal,
    Down,
    Armor,
    Invincible
}

/// <summary>
/// 実行時のステータスを管理する MonoBehaviour。
/// プレイヤー・敵ともにこのコンポーネントをアタッチする。
///
/// ■ 使い方
///   1. CharacterStatData を Inspector の StatData に設定する
///   2. TakeDamage(DamageInfo) を呼ぶとダメージ計算・HP 更新・死亡判定が行われる
///   3. OnDeath イベントを購読して死亡時の処理を行う
/// </summary>
public class CharacterStats : MonoBehaviour
{
    [Header("ステータスデータ")]
    [SerializeField] private CharacterStatData _statData;

    [Header("Hit Reaction Debug")]
    [SerializeField] private HitReactionState _hitReactionState = HitReactionState.Normal;
    [SerializeField] private Color _invincibleBlinkColor = Color.cyan;
    [SerializeField] private float _invincibleBlinkSpeed = 12f;

    private readonly List<DamageBuff> _damageBuffs = new List<DamageBuff>();
    private readonly List<MaterialPropertyBlock> _rendererBlocks = new List<MaterialPropertyBlock>();
    private Renderer[] _renderers;
    private Color[] _baseColors;
    private HitReactionState _lastAppliedHitReactionState;
    private HitReactionState _stateBeforeTimedState;
    private float _timedStateTimer;
    private bool _hasTimedState;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    // ---- プロパティ ----

    public CharacterStatData StatData => _statData;

    /// <summary>現在の HP</summary>
    public int CurrentHP { get; private set; }

    /// <summary>最大 HP</summary>
    public int MaxHP => _statData != null ? _statData.MaxHP : 0;

    /// <summary>死亡しているか</summary>
    public bool IsDead => CurrentHP <= 0;

    /// <summary>現在防御アクション中か。GuardState.Enter/Exit で更新する。</summary>
    public bool IsGuarding { get; set; }

    public HitReactionState CurrentHitReactionState => _hitReactionState;
    public bool IsInvincible => _hitReactionState == HitReactionState.Invincible;
    public bool IsArmor => _hitReactionState == HitReactionState.Armor;
    public bool IsDown => _hitReactionState == HitReactionState.Down;
    public bool CanReceiveHit => !IsDead && !IsInvincible;
    public bool CanBeKnockedBack => !IsDead && !IsInvincible && !IsArmor;

    public float OutgoingDamageMultiplier => GetDamageMultiplier(DamageBuffTarget.OutgoingDamage);

    public float IncomingDamageMultiplier => GetDamageMultiplier(DamageBuffTarget.IncomingDamage);

    // ---- イベント ----

    /// <summary>ダメージを受けた時のイベント。（実ダメージ量, クリティカルか, 攻撃の属性）</summary>
    public event Action<int, bool, AttributeType> OnDamaged;

    /// <summary>HP が 0 になった時のイベント</summary>
    public event Action OnDeath;

    public event Action OnDamageBuffsChanged;

    // ---- Unity ライフサイクル ----

    private void Awake()
    {
        CacheRenderers();

        if (_statData == null)
        {
            Debug.LogWarning($"[CharacterStats] {gameObject.name} に StatData が設定されていません。");
            return;
        }
        CurrentHP = _statData.MaxHP;
    }

    private void Update()
    {
        TickDamageBuffs(Time.deltaTime);
        TickTimedHitReactionState(Time.deltaTime);
        UpdateHitReactionVisual();
    }

    // ---- 公開 API ----

    /// <summary>
    /// ダメージを受ける。
    /// DamageHitEffect から呼ぶ。
    /// </summary>
    public void TakeDamage(DamageInfo info)
    {
        if (IsDead) return;
        if (_statData == null) return;
        if (!CanReceiveHit) return;

        // 防御中フラグを DamageInfo に反映
        info.IsGuarded = IsGuarding;
        info.IncomingDamageMultiplier = IncomingDamageMultiplier;
        info.UseIncomingDamageMultiplier = true;
        info.ForceWeakAttribute = IsDown;

        // ダメージ計算
        int damage = DamageCalculator.Calculate(info, _statData, out bool isCritical);

        // HP を減算
        CurrentHP = Mathf.Max(0, CurrentHP - damage);

        // ダメージイベント発火（HP 更新後）
        OnDamaged?.Invoke(damage, isCritical, info.Attribute);

        // 死亡判定
        if (CurrentHP <= 0)
            OnDeath?.Invoke();
    }

    /// <summary>Inspector デバッグや外部処理から通常の被弾状態を切り替える。</summary>
    public void SetHitReactionState(HitReactionState state)
    {
        _hasTimedState = false;
        _timedStateTimer = 0f;
        _hitReactionState = state;
    }

    /// <summary>回避無敵やダウンなど、一時的な被弾状態を設定する。</summary>
    public void SetTimedHitReactionState(HitReactionState state, float duration)
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

    public ResistanceLevel GetEffectiveAttributeResistanceLevel(AttributeType attribute)
    {
        if (IsDown) return ResistanceLevel.Weak;
        return _statData != null ? _statData.GetAttributeResistanceLevel(attribute) : ResistanceLevel.Normal;
    }

    public DamageBuff AddDamageBuff(
        DamageBuffTarget target,
        float multiplier,
        float duration = -1f,
        string id = null,
        bool overwriteMultiplier = true,
        bool overwriteDuration = true)
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
        _timedStateTimer = 0f;
        _hasTimedState = false;
    }

    private void CacheRenderers()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _baseColors = new Color[_renderers.Length];
        _rendererBlocks.Clear();

        for (int i = 0; i < _renderers.Length; i++)
        {
            Renderer renderer = _renderers[i];
            Material sharedMaterial = renderer.sharedMaterial;
            _baseColors[i] = sharedMaterial != null && sharedMaterial.HasProperty(BaseColorId)
                ? sharedMaterial.GetColor(BaseColorId)
                : sharedMaterial != null && sharedMaterial.HasProperty(ColorId)
                    ? sharedMaterial.GetColor(ColorId)
                    : Color.white;
            _rendererBlocks.Add(new MaterialPropertyBlock());
        }

        _lastAppliedHitReactionState = _hitReactionState;
    }

    private void UpdateHitReactionVisual()
    {
        if (_renderers == null || _renderers.Length == 0) return;

        bool shouldBlink = IsInvincible;
        if (!shouldBlink && _lastAppliedHitReactionState == _hitReactionState) return;

        for (int i = 0; i < _renderers.Length; i++)
        {
            Renderer renderer = _renderers[i];
            if (renderer == null) continue;

            MaterialPropertyBlock block = _rendererBlocks[i];
            renderer.GetPropertyBlock(block);

            Color color = shouldBlink
                ? Color.Lerp(_baseColors[i], _invincibleBlinkColor, Mathf.PingPong(Time.time * _invincibleBlinkSpeed, 1f))
                : _baseColors[i];

            // URP と Built-in の両方で色変更できるように代表的な色プロパティへ反映する。
            block.SetColor(BaseColorId, color);
            block.SetColor(ColorId, color);
            renderer.SetPropertyBlock(block);
        }

        _lastAppliedHitReactionState = _hitReactionState;
    }
}
