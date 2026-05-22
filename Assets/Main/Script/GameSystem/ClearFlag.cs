using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public class ClearFlag : MonoBehaviour
{
    private CharacterStats PlayerStatus;
    private List<CharacterStats> EnemyStatus = new List<CharacterStats>();
    [SerializeField] KillCamera camera;

    private bool GameOverFlag = false;
    private bool IsClear;

    private void Start()
    {
        PlayerStatus = 
            GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterStats>();
        GameObject[] EnemyObj = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject obj in EnemyObj)
        {
            if (obj.GetComponent<CharacterStats>() != null) {
                EnemyStatus.Add(obj.GetComponent<CharacterStats>());
            }
        }
        Debug.Log(EnemyStatus.Count);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameOverFlag) return;
        if (EnemyStatus.Count != 1)
        {
            for (int i = 0; i < EnemyStatus.Count; i++)
            {
                Debug.Log(i);
                if (EnemyStatus[i].CurrentHP <= 0)
                {
                    EnemyStatus.Remove(EnemyStatus[i]);
                }
            }
        }
        else
        {
            if (EnemyStatus[0].CurrentHP <= 0)
            {
                Debug.Log("ÅŒã‚Ìˆêl‚ªŽ€‚ñ‚¾II");
                camera.ActivateKillCam(EnemyStatus[0].transform);
                GameOverFlag = true;
            }
        }
        
    }
}
