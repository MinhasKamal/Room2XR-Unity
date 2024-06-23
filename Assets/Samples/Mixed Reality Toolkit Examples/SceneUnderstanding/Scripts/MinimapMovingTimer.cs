using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class MinimapMovingTimer : MonoBehaviour
{
    private GameObject minimapCamera;
    private Vector3 previousRotation;
    private DateTime MiniMapTime;
    private int rotationFlag;
    private int minimapFlag;
    // Start is called before the first frame update
    void Start()
    {
        minimapCamera = GameObject.Find("Minimap Camera");
        previousRotation = new Vector3(180, 0, 0);
        rotationFlag = 0;
        minimapFlag = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (minimapFlag == 1)
            CameraRotationTimer();
    }

    public void CameraRotationTimer()
    {
        if (Camera.main.transform.localEulerAngles.y != minimapCamera.transform.localEulerAngles.y)
        {
            MiniMapTime = DateTime.Now;
            previousRotation = Camera.main.transform.localEulerAngles;
            rotationFlag = 1;
        }
        if (rotationFlag == 1)
        {
            if (Mathf.Abs(previousRotation.y - minimapCamera.transform.localEulerAngles.y) < 0.01f)
            {
                print(Mathf.Abs(previousRotation.y - minimapCamera.transform.localEulerAngles.y));
                print(MiniMapTime.Subtract(DateTime.Now));
                rotationFlag = 0;
            }
        }
    }

    public void ToggleMinimapRotationFlag()
    {
        if (minimapFlag == 0)
            minimapFlag = 1;
        else
            minimapFlag = 0;
    }
}
