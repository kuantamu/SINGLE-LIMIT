using UnityEngine;

public class EnemyGenerator : MonoBehaviour
{
    [SerializeField] GameObject EnemyPrefab;
    [SerializeField] NpcTypeData[] NpcTypes;
    [SerializeField] float SpownSecond = 2;
    [SerializeField] bool RandomizeNpcType = true;
    private GameObject Enemy;
    private bool _canSpawn;

    private void Start()
    {
        _canSpawn = HasSpawnSource();
        if (!_canSpawn){
            Debug.LogWarning("EnemyGeneratorに敵が設定されていません！！");
            return;
        }
    }

    private void Update()
    {
        if (!_canSpawn) return;

        if (Enemy == null)
        {
            SpownSecond -= Time.deltaTime;
            if(SpownSecond <= 0)
            {
                SpownSecond = 2;
                EnemySpown();
            }
        }
    }

    private void EnemySpown()
    {
        NpcTypeData type = GetNpcType();
        GameObject prefab = type != null && type.Prefab != null ? type.Prefab : EnemyPrefab;

        if (prefab == null)
        {
            Debug.LogWarning("EnemyGeneratorにEnemyタグ付きPrefabが設定されていません！！");
            return;
        }

        Quaternion rotation = transform.rotation;
        Enemy = Instantiate(prefab, transform.position, rotation);

        if (type == null) return;

        NpcTypeApplier applier = Enemy.GetComponent<NpcTypeApplier>();
        if (applier == null)
            applier = Enemy.AddComponent<NpcTypeApplier>();

        applier.Apply(type);
    }

    private bool HasSpawnSource()
    {
        if (EnemyPrefab != null && EnemyPrefab.CompareTag("Charactor")) return true;
        if (NpcTypes == null) return false;

        for (int i = 0; i < NpcTypes.Length; i++)
        {
            if (NpcTypes[i] == null) continue;
            if (NpcTypes[i].Prefab == null && EnemyPrefab != null && EnemyPrefab.CompareTag("Charactor")) return true;
            if (NpcTypes[i].Prefab != null && NpcTypes[i].Prefab.CompareTag("Charactor"))
                return true;
        }

        return false;
    }

    private NpcTypeData GetNpcType()
    {
        if (NpcTypes == null || NpcTypes.Length == 0) return null;
        if (!RandomizeNpcType)
            return GetFirstValidNpcType();

        float totalWeight = 0f;
        for (int i = 0; i < NpcTypes.Length; i++)
        {
            if (!IsValidNpcType(NpcTypes[i])) continue;
            totalWeight += Mathf.Max(0f, NpcTypes[i].SpawnWeight);
        }

        if (totalWeight <= 0f) return null;

        float roll = Random.Range(0f, totalWeight);
        for (int i = 0; i < NpcTypes.Length; i++)
        {
            if (!IsValidNpcType(NpcTypes[i])) continue;

            roll -= Mathf.Max(0f, NpcTypes[i].SpawnWeight);
            if (roll <= 0f)
                return NpcTypes[i];
        }

        return null;
    }

    private NpcTypeData GetFirstValidNpcType()
    {
        for (int i = 0; i < NpcTypes.Length; i++)
        {
            if (IsValidNpcType(NpcTypes[i]))
                return NpcTypes[i];
        }

        return null;
    }

    private bool IsValidNpcType(NpcTypeData type)
    {
        if (type == null) return false;
        if (type.Prefab != null) return type.Prefab.CompareTag("Charactor");
        return EnemyPrefab != null && EnemyPrefab.CompareTag("Charactor");
    }
}
