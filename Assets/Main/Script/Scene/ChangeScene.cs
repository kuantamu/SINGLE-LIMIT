using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public void ButtonPushChangeScene()
    {
        SceneManager.LoadScene("MainScene");
    }
}
