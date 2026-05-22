using UnityEngine;

public class GameStartButton : MonoBehaviour
{
    [SerializeField] GameObject nextUI;
    [SerializeField] GameObject startUI;

    bool StageSelect = false;

    private void Start()
    {
        StageSelect = false;
        nextUI.SetActive(StageSelect);
    }

    public void StartButtonPush()
    {
        StageSelect = true;
        startUI.SetActive(false);
        nextUI.SetActive(StageSelect);
    }
}
