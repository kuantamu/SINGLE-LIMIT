using UnityEngine;

/// <summary>
/// 攻撃がヒットした時に実行される効果のインターフェース。
/// 新しい効果を追加する際はこのインターフェースを実装する。
///
/// ■ 実装例
///   DebugLogHitEffect  : ヒットしたオブジェクト名をデバッグログに出す
///   HitPointEffect     : レイで着弾点を取得してエフェクトを生成する
///   DamageHitEffect    : ダメージを与える（将来実装）
/// </summary>
public interface IHitEffect
{
    /// <summary>
    /// ヒット時に実行する処理。
    /// </summary>
    /// <param name="hitCollider">ヒットした対象の Collider</param>
    /// <param name="attacker">攻撃者の Transform（レイの発射元に使う）</param>
    void Execute(Collider hitCollider, Transform attacker);
}
