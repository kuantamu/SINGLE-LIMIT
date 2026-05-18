using UnityEngine;

/// <summary>
/// ロックオン関連の共通ユーティリティ。
/// ターゲットのルート取得・有効性チェック・照準点算出など、
/// LockOnController と ThirdPersonCamera の両方から利用される静的ヘルパークラス。
/// </summary>
public static class LockOnTargetUtility
{
    /// <summary>
    /// コライダーからロックオン対象となるルート Transform を取得する。
    /// 優先順位: EnemyStateMachine コンポーネント → "Enemy" タグ → 階層の最上位 root
    /// </summary>
    /// <param name="collider">OverlapSphere などで検出したコライダー</param>
    /// <returns>ロックオン対象のルート Transform（取得できない場合は null）</returns>
    public static Transform GetTargetRoot(Collider collider)
    {
        if (collider == null) return null;

        // 最優先: EnemyStateMachine を持つオブジェクトをルートとして扱う
        EnemyStateMachine enemy = collider.GetComponentInParent<EnemyStateMachine>();
        if (enemy != null) return enemy.transform;

        // 次点: "Enemy" タグが付いた祖先オブジェクトを探す
        Transform current = collider.transform;
        while (current != null)
        {
            if (current.CompareTag("Enemy"))
                return current;

            current = current.parent;
        }

        // 見つからなければ Transform の最上位（root）を返す
        return collider.transform.root;
    }

    /// <summary>
    /// 指定の Transform がロックオン可能な有効な敵かどうかを判定する。
    /// null チェック・アクティブ状態・死亡状態を確認する。
    /// </summary>
    /// <param name="target">チェック対象の Transform</param>
    /// <returns>ロックオン可能であれば true</returns>
    public static bool IsValidEnemy(Transform target)
    {
        // null またはゲームオブジェクトが非アクティブなら無効
        if (target == null || !target.gameObject.activeInHierarchy) return false;

        // CharacterStats を持つ場合、死亡済みなら無効（持たない場合はロックオン許可）
        CharacterStats stats = target.GetComponentInChildren<CharacterStats>();
        return stats == null || !stats.IsDead;
    }

    /// <summary>
    /// ロックオンのカメラが向くべき照準点（ワールド座標）を返す。
    /// コライダーの bounds.center → レンダラーの bounds.center → position + Vector3.up の順で取得を試みる。
    /// </summary>
    /// <param name="target">照準を合わせる対象</param>
    /// <returns>対象の中心付近のワールド座標</returns>
    public static Vector3 GetAimPoint(Transform target)
    {
        if (target == null) return Vector3.zero;

        Bounds bounds;

        // まずコライダーの合成 bounds を試みる
        bool hasBounds = TryGetColliderBounds(target, out bounds)
            || TryGetRendererBounds(target, out bounds);  // 次にレンダラーの合成 bounds を試みる

        // いずれかで取得できたら中心を返す
        if (hasBounds)
            return bounds.center;

        // どちらも取得できなかった場合は足元より1m上を返す（最低限のフォールバック）
        return target.position + Vector3.up;
    }

    /// <summary>
    /// 子孫のコライダー（isTrigger を除く有効なもの）を全て合成した Bounds を計算する。
    /// </summary>
    /// <param name="target">対象の Transform</param>
    /// <param name="bounds">合成結果の Bounds（取得失敗時は default）</param>
    /// <returns>有効なコライダーが1つ以上あれば true</returns>
    private static bool TryGetColliderBounds(Transform target, out Bounds bounds)
    {
        Collider[] colliders = target.GetComponentsInChildren<Collider>();
        bounds = default;
        bool hasBounds = false;

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider col = colliders[i];

            // 無効・非アクティブ・トリガーのコライダーはスキップ
            if (col == null || !col.enabled || col.isTrigger) continue;

            if (!hasBounds)
            {
                // 最初の有効コライダーで初期化
                bounds = col.bounds;
                hasBounds = true;
            }
            else
            {
                // 2つ目以降は Encapsulate で結合（全体を包む bounds を作る）
                bounds.Encapsulate(col.bounds);
            }
        }

        return hasBounds;
    }

    /// <summary>
    /// 子孫のレンダラー（有効なもの）を全て合成した Bounds を計算する。
    /// コライダーが存在しないオブジェクト向けのフォールバック。
    /// </summary>
    /// <param name="target">対象の Transform</param>
    /// <param name="bounds">合成結果の Bounds（取得失敗時は default）</param>
    /// <returns>有効なレンダラーが1つ以上あれば true</returns>
    private static bool TryGetRendererBounds(Transform target, out Bounds bounds)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        bounds = default;
        bool hasBounds = false;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];

            // 無効なレンダラーはスキップ
            if (renderer == null || !renderer.enabled) continue;

            if (!hasBounds)
            {
                // 最初の有効レンダラーで初期化
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                // 2つ目以降は Encapsulate で結合
                bounds.Encapsulate(renderer.bounds);
            }
        }

        return hasBounds;
    }
}
