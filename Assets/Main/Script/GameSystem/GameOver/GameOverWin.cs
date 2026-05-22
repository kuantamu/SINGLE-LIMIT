using UnityEngine;

public class GameOverWin : MonoBehaviour
{
    [SerializeField] GameObject WinObj;
    public void StartWin()
    {
        WinObj.SetActive(true);
        Time.timeScale = 0.0f;
    }
}
