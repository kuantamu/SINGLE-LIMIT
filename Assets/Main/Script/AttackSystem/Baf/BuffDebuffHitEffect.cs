using System;
using UnityEngine;

/// <summary>
/// 攻撃ヒット時にバフ／デバフを対象キャラクターへ付与するヒットエフェクト。
/// IHitEffect を実装しており、武器やスキルのヒットエフェクトリストに追加して使用する。
/// Applications 配列に設定した BuffDebuffApplication の数だけ、ヒット時に効果を適用する。
/// </summary>
[Serializable]
public class BuffDebuffHitEffect : IHitEffect
{
    /// <summary>
    /// ヒット時に付与するバフ／デバフの設定リスト。
    /// Inspector から複数設定可能で、1回のヒットで複数のバフを同時に付与できる。
    /// </summary>
    public BuffDebuffApplication[] Applications;

    /// <summary>
    /// ヒット時に呼び出されるエントリポイント。
    /// ヒットしたコライダーの親から CharacterStats を取得し、
    /// Applications に設定された全バフを順番に適用する。
    /// </summary>
    /// <param name="hitCollider">攻撃が当たったコライダー</param>
    /// <param name="attacker">攻撃を行ったオブジェクトの Transform（未使用だが IHitEffect インターフェース要件）</param>
    public void Execute(Collider hitCollider, Transform attacker)
    {
        // ヒット対象の CharacterStats を取得（親階層も含めて検索）
        CharacterStats targetStats = hitCollider.GetComponentInParent<CharacterStats>();

        // Stats が存在しない、または Applications が未設定なら何もしない
        if (targetStats == null || Applications == null) return;

        // 設定された全バフ／デバフを順番に適用する
        for (int i = 0; i < Applications.Length; i++)
        {
            BuffDebuffApplication application = Applications[i];
            if (application == null) continue;

            // CharacterStats にバフを追加（既存バフがあれば上書きフラグに従い更新）
            targetStats.AddDamageBuff(
                application.BuffType,
                application.Multiplier,
                application.Duration,
                application.BuffKey,
                application.OverwriteMultiplier,
                application.OverwriteDuration);
        }
    }
}
