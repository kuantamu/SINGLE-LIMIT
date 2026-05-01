using UnityEngine;

/// <summary>
/// 攻撃者からヒットした対象へレイを飛ばし、
/// 着弾点にエフェクトを生成する効果。
///
/// ■ レイの流れ
///   攻撃者の腰の高さ → ヒットした対象の中心方向 → 最初に当たった点にエフェクト生成
/// </summary>
[System.Serializable]
public class HitPointEffect : IHitEffect
{
    /// <summary>エフェクトの設定（ScriptableObject）</summary>
    public HitPointEffectData Data;

    public void Execute(Collider hitCollider, Transform attacker)
    {
        if (Data == null || Data.EffectPrefab == null) return;

        Vector3 hitPoint = FindHitPoint(hitCollider, attacker);
        SpawnEffect(hitPoint, hitCollider);
    }

    // ---- 内部 ----

    /// <summary>
    /// 攻撃者から対象へレイを飛ばして着弾点を取得する。
    /// レイが当たらない場合は対象の bounds の中心を使用する。
    /// </summary>
    private Vector3 FindHitPoint(Collider hitCollider, Transform attacker)
    {
        // 攻撃者の腰の高さからレイを発射（目線が低すぎたり高すぎたりしないよう）
        Vector3 origin = attacker.position + Vector3.up * 0.8f;
        Vector3 target = hitCollider.bounds.center;
        Vector3 dir    = (target - origin).normalized;
        float   dist   = Data.RayMaxDistance;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, Data.RayLayer))
        {
            return hit.point;
        }

        // レイが外れた場合は対象の中心を使用
        return target;
    }

    /// <summary>着弾点にエフェクトを生成する</summary>
    private void SpawnEffect(Vector3 position, Collider hitCollider)
    {
        // 対象の表面の法線方向へエフェクトを向ける
        Vector3 normal    = (position - hitCollider.bounds.center).normalized;
        Quaternion rot    = normal != Vector3.zero
            ? Quaternion.LookRotation(normal)
            : Quaternion.identity;

        GameObject effect = Object.Instantiate(Data.EffectPrefab, position, rot);

        if (Data.AutoDestroyTime > 0f)
            Object.Destroy(effect, Data.AutoDestroyTime);
    }
}
