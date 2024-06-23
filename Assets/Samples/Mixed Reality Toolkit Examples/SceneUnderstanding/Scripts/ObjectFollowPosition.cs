using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFollowPosition : MonoBehaviour
{
    private GameObject MinimapCamera;
    public static float cameraYDistance;
    // Start is called before the first frame update
    void Start()
    {
        MinimapCamera = GameObject.Find("Minimap Camera");
        cameraYDistance = 25.0f;
    }

    // Update is called once per frame
    void Update()
    {
        MinimapCamera.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y + cameraYDistance, Camera.main.transform.position.z);
        MinimapCamera.transform.eulerAngles = new Vector3(MinimapCamera.transform.eulerAngles.x, Camera.main.transform.eulerAngles.y, MinimapCamera.transform.eulerAngles.z);
    }
}
