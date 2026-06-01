using UnityEngine;

public static class LockOnTargetUtility
{
    public static Transform GetTargetRoot(Collider collider)
    {
        if (collider == null) return null;

        CharacterStats stats = collider.GetComponentInParent<CharacterStats>();
        if (stats != null) return stats.transform;

        return collider.transform.root;
    }

    public static bool IsValidEnemy(Transform target)
    {
        return IsValidEnemy(target, null);
    }

    public static bool IsValidEnemy(Transform target, CharacterStats seeker)
    {
        if (target == null || !target.gameObject.activeInHierarchy) return false;

        CharacterStats stats = target.GetComponentInChildren<CharacterStats>();
        if (stats == null || stats.IsDead) return false;

        return seeker == null || CharacterFactionRules.IsHostile(seeker, stats);
    }

    public static Vector3 GetAimPoint(Transform target)
    {
        if (target == null) return Vector3.zero;

        Bounds bounds;
        bool hasBounds = TryGetColliderBounds(target, out bounds)
            || TryGetRendererBounds(target, out bounds);

        if (hasBounds)
            return bounds.center;

        return target.position + Vector3.up;
    }

    private static bool TryGetColliderBounds(Transform target, out Bounds bounds)
    {
        Collider[] colliders = target.GetComponentsInChildren<Collider>();
        bounds = default;
        bool hasBounds = false;

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider col = colliders[i];
            if (col == null || !col.enabled || col.isTrigger) continue;

            if (!hasBounds)
            {
                bounds = col.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(col.bounds);
            }
        }

        return hasBounds;
    }

    private static bool TryGetRendererBounds(Transform target, out Bounds bounds)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        bounds = default;
        bool hasBounds = false;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null || !renderer.enabled) continue;

            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        return hasBounds;
    }
}
