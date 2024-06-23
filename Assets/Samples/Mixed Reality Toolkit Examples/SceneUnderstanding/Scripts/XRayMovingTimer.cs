using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class XRayMovingTimer : MonoBehaviour
{
    public static int xRayTimerFlag;
    // Start is called before the first frame update
    void Start()
    {
        xRayTimerFlag = 0;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void toggleXRayTimer()
    {
        if (xRayTimerFlag == 0)
            xRayTimerFlag = 1;
        else
            xRayTimerFlag = 0;
    }

}
