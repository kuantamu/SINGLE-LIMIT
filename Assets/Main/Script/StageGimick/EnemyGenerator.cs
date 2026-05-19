using UnityEngine;

public class EnemyGenerator : MonoBehaviour
{
    [SerializeField] GameObject EnemyPrefab;
    [SerializeField] float SpownSecond = 2;
    private GameObject Enemy;

    private void Start()
    {
        if (EnemyPrefab == null || EnemyPrefab.tag != "Enemy"){
            Debug.LogWarning("EnemyGenerator‚É“G‚ªİ’è‚³‚ê‚Ä‚¢‚Ü‚¹‚ñII");
            return;
        }
    }

    private void Update()
    {
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
        Enemy = Instantiate(EnemyPrefab, transform.position, Quaternion.identity);
    }
}
