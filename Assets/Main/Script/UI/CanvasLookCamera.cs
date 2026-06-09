using UnityEngine;

public class CanvasLookCamera : MonoBehaviour
{
    Camera _maincam;

    void Start()
    {
        _maincam = Camera.main;
    }

    void Update()
    {
        transform.LookAt(_maincam.transform);
    }
}
