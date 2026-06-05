using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public class ClearFlag : MonoBehaviour
{
    private CharacterStats PlayerStatus;
    private List<CharacterStats> EnemyStatus = new List<CharacterStats>();
    private List<CharacterStats> OtherStatus = new List<CharacterStats>();
    [SerializeField] KillCamera killCamera;

    private bool GameOverFlag = false;
    private bool IsClear;

    private void Start()
    {

        GameObject[] CharaObj = GameObject.FindGameObjectsWithTag("Charactor");
        foreach (GameObject obj in CharaObj)
        {
            if (obj.GetComponent<CharacterStats>() != null) {
                switch (obj.GetComponent<CharacterStats>().Faction)
                {
                    case CharacterFaction.Player:
                        PlayerStatus = obj.GetComponent<CharacterStats>();
                        break;
                    case CharacterFaction.Enemy:
                        EnemyStatus.Add(obj.GetComponent<CharacterStats>());
                        break;
                    default:
                        OtherStatus.Add(obj.GetComponent<CharacterStats>());
                        break;
                }
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
                killCamera.ActivateKillCam(EnemyStatus[0].transform);
                GameOverFlag = true;
            }
        }
        
    }
}
