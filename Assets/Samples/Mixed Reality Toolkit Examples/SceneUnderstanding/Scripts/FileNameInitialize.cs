using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class FileNameInitialize : MonoBehaviour
{
    public static StreamWriter writer;
    private int fileCounter;
    // Start is called before the first frame update
    void Start()
    {
        fileCounter = 0;
        string combinedPath = Path.Combine(Application.persistentDataPath, "_ObjectCreationTime.txt");
        while (File.Exists(combinedPath) == true)
        {
            combinedPath = Path.Combine(Application.persistentDataPath, fileCounter.ToString() + "_ObjectCreationTime.txt");
            fileCounter = fileCounter + 1;
        }
        print(combinedPath);
        writer = new StreamWriter(combinedPath, true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDestroy()
    {
        writer.Close();
    }
}
