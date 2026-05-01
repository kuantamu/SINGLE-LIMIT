using UnityEngine;

/// <summary>
/// ヒット時にダメージ数字ポップアップを生成する IHitEffect 実装。
/// 設定は Inspector から変更可能だが、すべてデフォルト値があるため
/// 何も設定しなくても動作する。
/// </summary>
[System.Serializable]
public class DamageNumberEffect : IHitEffect
{
    [Header("表示位置")]
    public float SpawnOffsetY  = 0.5f;
    public float RandomOffsetX = 0.3f;

    [Header("演出")]
    public float FloatSpeed = 1.2f;
    public float Duration   = 0.8f;

    [Header("色")]
    public Color NormalColor   = Color.white;
    public Color CriticalColor = new Color(1f, 0.85f, 0f);
    public Color GuardColor    = new Color(0.6f, 0.6f, 1f);

    [Header("フォントサイズ")]
    public float NormalFontSize   = 10f;
    public float CriticalFontSize = 15f;
    public float GuardFontSize    = 7f;

    public void Execute(Collider hitCollider, Transform attacker)
    {
        var stats = hitCollider.GetComponentInParent<CharacterStats>();
        if (stats == null) return;

        System.Action<int, bool,AttributeType> handler = null;
        handler = (damage, isCritical, damageType) =>
        {
            Debug.Log(stats.StatData.attributeResistanceLevel(damageType));
            stats.OnDamaged -= handler;
            SpawnPopup(hitCollider, damage, isCritical, stats.IsGuarding,damageType, 
                stats.StatData.attributeResistanceLevel(damageType));
            
        };

        stats.OnDamaged += handler;
    }

    private void SpawnPopup(Collider hitCollider, int damage, bool isCritical, 
        bool isGuarded, AttributeType damageType, ResistanceLevel resistanceLevel)
    {
        
        var prefab = DamageNumberPopup.GetPrefab();
        if (prefab == null) return;

        Vector3 spawnPos = hitCollider.bounds.center
            + Vector3.up * (hitCollider.bounds.extents.y + SpawnOffsetY)
            + Vector3.right * Random.Range(-RandomOffsetX, RandomOffsetX);

        var obj   = Object.Instantiate(prefab, spawnPos, Quaternion.identity);
        var popup = obj.GetComponent<DamageNumberPopup>();
        if (popup == null) return;

        Color color;
        float fontSize;

        if (isGuarded)
        {
            color    = GuardColor;
            fontSize = GuardFontSize;
        }
        else if (isCritical)
        {
            color    = CriticalColor;
            fontSize = CriticalFontSize;
        }
        else
        {
            color    = NormalColor;
            fontSize = NormalFontSize;
        }

        popup.Init(damage, color, fontSize, FloatSpeed, Duration, damageType, resistanceLevel);
    }
}
