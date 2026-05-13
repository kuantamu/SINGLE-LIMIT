using System;
using System.Collections.Generic;
using UnityEngine;

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

    private readonly List<DamageBuff> _damageBuffs = new List<DamageBuff>();

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

        // 防御中フラグを DamageInfo に反映
        info.IsGuarded = IsGuarding;
        info.IncomingDamageMultiplier = IncomingDamageMultiplier;
        info.UseIncomingDamageMultiplier = true;

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
}
