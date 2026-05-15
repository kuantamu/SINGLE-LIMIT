using UnityEngine;

public static class LockOnTargetUtility
{
    public static Transform GetTargetRoot(Collider collider)
    {
        if (collider == null) return null;

        EnemyStateMachine enemy = collider.GetComponentInParent<EnemyStateMachine>();
        if (enemy != null) return enemy.transform;

        Transform current = collider.transform;
        while (current != null)
        {
            if (current.CompareTag("Enemy"))
                return current;

            current = current.parent;
        }

        return collider.transform.root;
    }

    public static bool IsValidEnemy(Transform target)
    {
        if (target == null || !target.gameObject.activeInHierarchy) return false;

        CharacterStats stats = target.GetComponentInChildren<CharacterStats>();
        return stats == null || !stats.IsDead;
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
