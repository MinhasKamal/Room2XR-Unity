using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class VEUpdateTimer : MonoBehaviour
{
    private GameObject objectSet;
    public string targetName;
    private int previousCount;
    // Start is called before the first frame update
    void Start()
    {
        previousCount = 999;
    }

    // Update is called once per frame
    void Update()
    {
        findUpdateTime(targetName);
    }

    public void findUpdateTime(string objectName)
    {
        objectSet = GameObject.Find(objectName);
        if (objectSet)
        {
            //if (previousCount != objectSet.transform.childCount)
            //{
            //    print(objectSet.transform.childCount);
            //    print(DateTime.Now.ToString("h:mm:ss.ffffff tt"));
            //}
            previousCount = objectSet.transform.childCount;
        }
    }
}
