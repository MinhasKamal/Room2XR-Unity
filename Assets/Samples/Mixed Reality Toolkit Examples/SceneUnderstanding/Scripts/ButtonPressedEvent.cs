using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class ButtonPressedEvent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void buttonPressedTime()
    {
        print(DateTime.Now.ToString("h:mm:ss.ffffff tt"));
    }
}
