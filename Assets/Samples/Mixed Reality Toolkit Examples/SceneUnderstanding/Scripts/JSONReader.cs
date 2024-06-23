using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JSONReader : MonoBehaviour
{
    public TextAsset testJSON;

    // public static object[,] JsonDataArray;
    public static object[] JsonDataArray;
    private object[] DataTempArray;

    // Start is called before the first frame update
    void Start()
    {
        JsonSceneData loadedJsonSceneData = JsonUtility.FromJson<JsonSceneData>(testJSON.text);

        JsonDataArray = new object[loadedJsonSceneData.objects.Length * 25];
        // DataTempArray = new object[24];

        // print(loadedJsonSceneData.objects.Length);

        for (int i = 0; i < loadedJsonSceneData.objects.Length; i++)
        {
            JsonDataArray[i * 25 + 0] = loadedJsonSceneData.objects[i].scale;
            JsonDataArray[i * 25 + 1] = loadedJsonSceneData.objects[i].scale3d.x;
            JsonDataArray[i * 25 + 2] = loadedJsonSceneData.objects[i].scale3d.y;
            JsonDataArray[i * 25 + 3] = loadedJsonSceneData.objects[i].scale3d.z;
            JsonDataArray[i * 25 + 4] = loadedJsonSceneData.objects[i].center.x;
            JsonDataArray[i * 25 + 5] = loadedJsonSceneData.objects[i].center.y;
            JsonDataArray[i * 25 + 6] = loadedJsonSceneData.objects[i].center.z;
            JsonDataArray[i * 25 + 7] = loadedJsonSceneData.objects[i].translation.x;
            JsonDataArray[i * 25 + 8] = loadedJsonSceneData.objects[i].translation.y;
            JsonDataArray[i * 25 + 9] = loadedJsonSceneData.objects[i].translation.z;
            JsonDataArray[i * 25 + 10] = loadedJsonSceneData.objects[i].objCenter.x;
            JsonDataArray[i * 25 + 11] = loadedJsonSceneData.objects[i].objCenter.y;
            JsonDataArray[i * 25 + 12] = loadedJsonSceneData.objects[i].objCenter.z;
            JsonDataArray[i * 25 + 13] = loadedJsonSceneData.objects[i].rotation.w;
            JsonDataArray[i * 25 + 14] = loadedJsonSceneData.objects[i].rotation.x;
            JsonDataArray[i * 25 + 15] = loadedJsonSceneData.objects[i].rotation.y;
            JsonDataArray[i * 25 + 16] = loadedJsonSceneData.objects[i].rotation.z;
            JsonDataArray[i * 25 + 17] = loadedJsonSceneData.objects[i].bbox.center;
            JsonDataArray[i * 25 + 18] = loadedJsonSceneData.objects[i].bbox.extent;
            JsonDataArray[i * 25 + 19] = loadedJsonSceneData.objects[i].path;
            JsonDataArray[i * 25 + 20] = loadedJsonSceneData.objects[i].color.r;
            JsonDataArray[i * 25 + 21] = loadedJsonSceneData.objects[i].color.g;
            JsonDataArray[i * 25 + 22] = loadedJsonSceneData.objects[i].color.b;
            JsonDataArray[i * 25 + 23] = loadedJsonSceneData.objects[i].color.a;

            JsonDataArray[i * 25 + 24] = loadedJsonSceneData.structures;

            //JsonDataArray[i, 0] = loadedJsonSceneData.objects[i].scale;
            //JsonDataArray[i, 1] = loadedJsonSceneData.objects[i].scale3d.x;
            //JsonDataArray[i, 2] = loadedJsonSceneData.objects[i].scale3d.y;
            //JsonDataArray[i, 3] = loadedJsonSceneData.objects[i].scale3d.z;
            //JsonDataArray[i, 4] = loadedJsonSceneData.objects[i].center.x;
            //JsonDataArray[i, 5] = loadedJsonSceneData.objects[i].center.y;
            //JsonDataArray[i, 6] = loadedJsonSceneData.objects[i].center.z;
            //JsonDataArray[i, 7] = loadedJsonSceneData.objects[i].translation.x;
            //JsonDataArray[i, 8] = loadedJsonSceneData.objects[i].translation.y;
            //JsonDataArray[i, 9] = loadedJsonSceneData.objects[i].translation.z;
            //JsonDataArray[i, 10] = loadedJsonSceneData.objects[i].objCenter.x;
            //JsonDataArray[i, 11] = loadedJsonSceneData.objects[i].objCenter.y;
            //JsonDataArray[i, 12] = loadedJsonSceneData.objects[i].objCenter.z;
            //JsonDataArray[i, 13] = loadedJsonSceneData.objects[i].rotation.w;
            //JsonDataArray[i, 14] = loadedJsonSceneData.objects[i].rotation.x;
            //JsonDataArray[i, 15] = loadedJsonSceneData.objects[i].rotation.y;
            //JsonDataArray[i, 16] = loadedJsonSceneData.objects[i].rotation.z;
            //JsonDataArray[i, 17] = loadedJsonSceneData.objects[i].bbox.center;
            //JsonDataArray[i, 18] = loadedJsonSceneData.objects[i].bbox.extent;
            //JsonDataArray[i, 19] = loadedJsonSceneData.objects[i].path;
            //JsonDataArray[i, 20] = loadedJsonSceneData.objects[i].color.r;
            //JsonDataArray[i, 21] = loadedJsonSceneData.objects[i].color.g;
            //JsonDataArray[i, 22] = loadedJsonSceneData.objects[i].color.b;
            //JsonDataArray[i, 23] = loadedJsonSceneData.objects[i].color.a;

            //JsonDataArray[i, 24] = loadedJsonSceneData.structures;
        }

        // print(JsonDataArray[0, 1]);

        // print(loadedJsonSceneData.objects[0].rotation.w);
        // print(JsonDataArray[0]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //[System.Serializable]
    //public class JsonSceneData
    //{
    //    public var objects;
    //    public int color;
    //}


    [System.Serializable]
    public class JsonSceneData
    {
        public JsonObjectData[] objects;
        public string[] structures;
    }

    [System.Serializable]
    public class JsonObjectData
    {
        public int scale;
        public Vector3 scale3d;
        public Vector3 center;
        public Vector3 translation;
        public Vector3 objCenter;
        public Vector4 rotation;
        public JsonBBoxData bbox;
        public string path;
        public Color color;
    }

    [System.Serializable]
    public class JsonBBoxData
    {
        public double[] center;
        public double[] extent;
    }
}
