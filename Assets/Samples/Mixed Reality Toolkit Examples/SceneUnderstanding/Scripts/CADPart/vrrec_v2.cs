using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Numerics;
using UnityEngine;
using Dummiesman;
using Unity.VisualScripting;
using UnityEditor;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;


public class vrrec_v2 : MonoBehaviour
{
    // public void Wait(float seconds, Action action){
    //     StartCoroutine(_wait(seconds, action));
    // }
    // IEnumerator _wait(float time, Action callback){
    //     yield return new WaitForSeconds(time);
    //     callback();
    // }
    
    // Classes
    [Serializable]
    public class ReconObject
    {
        public float scale = 1.0f;
        public Vector3 scale3d = Vector3.one;
        public Vector3 center = new Vector3();
        public Vector3 translation = new Vector3();
        public Color color = new Color();
        public float yaw = 0f;
        public string path = new string("");
        public string offPath = new string("");
        public GameObject obj;

        public void Load(GameObject parent)
        {
            var path_segs = path.Split('.');
            path_segs[path_segs.Length - 1] = "obj";
            offPath = String.Join('.', path_segs);

            if (!File.Exists(offPath))
            {
                Debug.Log("File doesn't exist: " + offPath);
            }
            else
            {
                if(obj != null)            
                    Destroy(obj);
                // Debug.Log("Loading object from: " + offPath);
                OBJLoader loader = new OBJLoader();
                obj  = LoadOFF(offPath); 
                // obj = loader.Load(offPath);
                obj.transform.SetParent(parent.transform, worldPositionStays:false);
                // Debug.Log("Loaded object from path " + path + "\nObj: " + obj + "\nMesh: ");
                // Debug.Log($"Parent game object for {path}: {obj.transform.parent.GameObject()}");
                ;
                ApplyTransform(obj, center, translation, scale3d, scale, yaw, color);
                Debug.Log("Loaded object: " + ToString());
            }
        }

        public GameObject LoadOFF(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            string dirPath = Path.GetDirectoryName(filePath);
            string objName = Path.GetFileNameWithoutExtension(filePath);
            GameObject obj = new GameObject();
            obj.name = objName;
            ConstructModel (obj, dirPath, fileName, false);
            
            return obj;
        }
        
        public void ConstructModel (GameObject gameObject, string directoryPath, string filename, bool load_mtl = false) {
            // isLoaded = false;

            FileReader.ObjectFile obj = FileReader.ReadObjectFile (Path.Combine(directoryPath, filename));
            FileReader.MaterialFile mtl;
            MeshFilter filter = gameObject.AddComponent<MeshFilter> ();
            MeshRenderer renderer = gameObject.AddComponent<MeshRenderer> ();
            if (load_mtl)
            {
                mtl = FileReader.ReadMaterialFile(directoryPath + obj.mtllib);
                renderer.materials = DefineMaterial (directoryPath, obj, mtl);
            }
            else
            {
                renderer.material = new Material (Shader.Find ("Diffuse"));
            }

            filter.mesh = PopulateMesh (obj);
            // isLoaded = true;
        }
        
        Material[] DefineMaterial (string directoryPath, FileReader.ObjectFile obj, FileReader.MaterialFile mtl) {

            Material[] materials = new Material[obj.usemtl.Count];

            for (int i = 0; i < obj.usemtl.Count; i += 1) {
                int index = mtl.newmtl.IndexOf (obj.usemtl [i]);

                Texture2D texture = new Texture2D (1, 1);
                texture.LoadImage (File.ReadAllBytes (directoryPath + mtl.mapKd [index]));

                materials [i] = new Material (Shader.Find ("Diffuse"));
                materials [i].name = mtl.newmtl [index];
                materials [i].mainTexture = texture;
            }

            return materials;
        }
        
        Mesh PopulateMesh (FileReader.ObjectFile obj) {

            Mesh mesh = new Mesh ();

            List<int[]> triplets = new List<int[]> ();
            List<int> submeshes = new List<int> ();

            for (int i = 0; i < obj.f.Count; i += 1) {
                for (int j = 0; j < obj.f [i].Count; j += 1) {
                    triplets.Add (obj.f [i] [j]);
                }
                submeshes.Add (obj.f [i].Count);
            }

            Vector3[] vertices = new Vector3[triplets.Count];
            Vector3[] normals = new Vector3[triplets.Count];
            Vector2[] uvs = new Vector2[triplets.Count];

            for (int i = 0; i < triplets.Count; i += 1) {
                vertices [i] = obj.v [triplets [i] [0] - 1];
                normals [i] = obj.vn [triplets [i] [2] - 1];
                if (triplets [i] [1] > 0)
                    uvs [i] = obj.vt [triplets [i] [1] - 1];
            }

            mesh.name = obj.o;
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.subMeshCount = submeshes.Count;

            int vertex = 0;
            for (int i = 0; i < submeshes.Count; i += 1) {
                int[] triangles = new int[submeshes [i]];
                for (int j = 0; j < submeshes [i]; j += 1) {
                    triangles [j] = vertex;
                    vertex += 1;
                }
                mesh.SetTriangles (triangles, i);
            }

            mesh.RecalculateBounds ();
            mesh.Optimize ();

            return mesh;
        }
        
        public override string ToString()
        {
            return "\tpath: " + path + "\n\tcolor: " + color + "\n\tscale: " + scale + "\n\tcenter: " + center.ToString("F4") + "\n\ttranslation: " + translation.ToString("F4") + "\n\tyaw: " + yaw;
        }
        
    }

    [Serializable]
    public class ReconLayout
    {
        public Vector3 scale3d = new Vector3();
        public Vector3 center = new Vector3();
        public Vector3 translation = new Vector3();
        public Color color = new Color();
        public float yaw = 0f;
        public GameObject obj;
        
        public void Load(GameObject parent)
        {
            obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.transform.SetParent(parent.transform, worldPositionStays:true);
            ApplyTransform(obj, center, translation, scale3d, 1, yaw, color);
            Debug.Log("Loaded structure: " + ToString());
        }
        
        // public void ApplyTransform()
        // {
        //     obj.transform.position = Vector3.zero;
        //     // obj.transform.position = center;
        //     obj.transform.localScale = scale3d;
        //     obj.transform.Rotate(0f, 0f, yaw, Space.Self);
        //     obj.GetComponent<Renderer>().material.color = color;
        // }
        
        public override string ToString()
        {
            return "\n\tcolor: " + color + "\n\tscale: " + scale3d.ToString("F4") + "\n\tcenter: " + center.ToString("F4") + "\n\ttranslation: " + translation.ToString("F4") + "\n\tyaw: " + yaw;
        }
    }

    [Serializable]
    public class ReconScene
    {
        public List<ReconObject> objects = new List<ReconObject>();
        public List<ReconLayout> structures = new List<ReconLayout>();
        
        public void Load()
        {
            GameObject scene = GameObject.FindGameObjectWithTag("Scene");
            scene.transform.position = Vector3.zero;
            scene.transform.localScale = Vector3.one;
            // Load all objects
            foreach (var obj in objects)
            {
                obj.Load(scene);
            }

            // Load all structures
            foreach (var structure in structures)
            {
                structure.Load(scene);
            }
            
            // Reposition light source
            var light = GameObject.Find("Point Light");
            Bounds worldBounds = FindBounds();
            Debug.Log("Found world bounds: " + worldBounds);
            light.transform.position = worldBounds.center + worldBounds.extents/2;
            
            // Reposition camera
            var camera = GameObject.Find("Main Camera");
            camera.transform.position = worldBounds.center + worldBounds.extents/2;;
            camera.transform.LookAt(Vector3.down);
        }

        public Bounds FindBounds()
        {
            var rnds = FindObjectsOfType<Renderer>();
            if (rnds.Length == 0)
                return new Bounds(Vector3.zero, Vector3.zero); // nothing to see here, go on
 
            var b = rnds[0].bounds;
            for (int i = 1; i < rnds.Length; i++)
                b.Encapsulate(rnds[i].bounds);
            
            return b;
        }
        
        public override string ToString()
        {
            string outStr = "Objects:\n";
            foreach (ReconObject obj in objects)
            {
                outStr += obj + "\n\n";
            }
            outStr += "Structures:\n";
            foreach (ReconLayout obj in structures)
            {
                outStr += obj + "\n\n";
            }
            return outStr;
        }
    }

    // Variables
    public string json_path;
    
    private static ReconScene scene;
    private ReconObject sampleObj;
    
    // Start is called before the first frame update
    void Start()
    {
        //string json;
        //var t_start = System.DateTime.Now;
        //using (StreamReader r = new StreamReader(json_path))
        //{
        //    json = r.ReadToEnd();
        //}
        
        //scene = new ReconScene();
        //scene = JsonUtility.FromJson<ReconScene>(json);
        
        //scene.Load();
        //var time_taken = System.DateTime.Now - t_start;
        //Debug.Log($"Time taken = {time_taken}");
    }

    public static void JsonSceneProcessor(string jsonString)
    {
        var t_start = System.DateTime.Now;

        scene = new ReconScene();
        scene = JsonUtility.FromJson<ReconScene>(jsonString);
        // Debug.Log(scene.objects.Count);
        // Debug.Log(scene.structures.Count);
        // Debug.Log(scene);

        scene.Load();
        var time_taken = System.DateTime.Now - t_start;
        Debug.Log($"Time taken = {time_taken}");
    }

    public static void ApplyTransform(GameObject obj, Vector3 center, Vector3 translation, Vector3 scale3d, float scale, float yaw, Color color)
    {
        // obj.transform.localScale = Vector3.one;
        obj.transform.localScale = scale3d * scale;
        // Debug.Log($"Set scale to {scale3d} x {scale}");
        obj.transform.Rotate(0f, 0f, yaw, Space.Self);
        // Debug.Log($"Set rotation to 0, 0, {yaw}");
        // obj.transform.position = center;
        obj.transform.position = Vector3.zero;
        // Debug.Log($"Set position to {center}");
        // obj.transform.SetPositionAndRotation(center, Quaternion.Euler(0, 0, yaw));
        // obj.transform.Translate(translation, Space.Self);
        // obj.transform.Rotate(0f, 0f, yaw+180, Space.World);
        obj.GetComponent<MeshRenderer>().material.color = color;
            
        // obj.transform.localScale = Vector3.one * scale * 1;
        // // obj.transform.Translate(translation, Space.Self);
        // obj.transform.Rotate(0f, 0f, yaw, Space.Self);
        // obj.transform.position = center;
        // obj.GetComponentInChildren<MeshRenderer>().material.color = color;
    }

    // Update is called once per frame
    void Update()
    {
        // MeshRenderer renderer = sampleObj.obj.GetComponent(typeof(MeshRenderer)) as MeshRenderer;
        // Debug.Log(renderer.isVisible);
    }
}
