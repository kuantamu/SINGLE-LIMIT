using System;
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

    // ---- イベント ----

    /// <summary>ダメージを受けた時のイベント。（実ダメージ量, クリティカルか）</summary>
    public event Action<int, bool, AttributeType> OnDamaged;

    /// <summary>HP が 0 になった時のイベント</summary>
    public event Action OnDeath;

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

        int damage = DamageCalculator.Calculate(info, _statData, out bool isCritical);

        //Debug.Log($"[CharacterStats] {gameObject.name} " +
        //          $"ダメージ: {damage}" +
        //          $"{(isCritical ? " [クリティカル]" : "")}" +
        //          $"{(info.IsGuarded ? " [ガード]" : "")}" +
        //          $" HP: {CurrentHP}/{MaxHP}");


        OnDamaged?.Invoke(damage, isCritical,info.Attribute);

        CurrentHP = Mathf.Max(0, CurrentHP - damage);

        if (CurrentHP <= 0)
            OnDeath?.Invoke();
    }
}
