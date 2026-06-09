using UnityEngine;

[System.Serializable]
public class HitPointEffect : IHitEffect
{
    public HitPointEffectData Data;

    public void Execute(Collider hitCollider, Transform attacker)
    {
        if (Data == null || Data.EffectPrefab == null) return;

        Vector3 hitPoint = FindHitPoint(hitCollider, attacker);
        SpawnEffect(hitPoint, hitCollider);
    }

    private Vector3 FindHitPoint(Collider hitCollider, Transform attacker)
    {
        Vector3 origin = attacker.position + Vector3.up * 0.8f;
        Vector3 target = hitCollider.bounds.center;
        Vector3 dir    = (target - origin).normalized;
        float   dist   = Data.RayMaxDistance;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, Data.RayLayer))
        {
            return hit.point;
        }

        return target;
    }

    private void SpawnEffect(Vector3 position, Collider hitCollider)
    {
        Vector3 normal    = (position - hitCollider.bounds.center).normalized;
        Quaternion rot    = normal != Vector3.zero
            ? Quaternion.LookRotation(normal)
            : Quaternion.identity;

        GameObject effect = Object.Instantiate(Data.EffectPrefab, position, rot);

        if (Data.AutoDestroyTime > 0f)
            Object.Destroy(effect, Data.AutoDestroyTime);
    }
}
