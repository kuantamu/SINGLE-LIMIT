using UnityEngine;

public class CanvasLookCamera : MonoBehaviour
{
    Camera _maincam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _maincam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(_maincam.transform);
    }
}
