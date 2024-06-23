using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

using Dummiesman;
using UnityEngine.Serialization;
// using Pcx;
// using ThreeDeeBear.Models.Ply;

using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using Vector4 = UnityEngine.Vector4;
using Matrix4x4 = UnityEngine.Matrix4x4;

using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEditor;

using Photon.Pun;
using Photon.Realtime;
using MRTK.Tutorials.MultiUserCapabilities;

public class vrrec : MonoBehaviour
{
    // Classes
    [Serializable]
    public class ReconObject
    {
        public Vector3 objCenter = new Vector3();
        public Color color = new Color();
        public string path = new string("");
        public string offPath = new string(""); // <--
        [FormerlySerializedAs("obj")] public GameObject go;
        public float[] t_mat; // <--
        public float[] vertex_colors;
        public Color[] vertexColors;
        public Vector3[] vertexPoints;
        public Matrix4x4 tMat;

        private GameObject TCPServerGameObject;
        public void Load(GameObject parent, bool colorModels, string databasePath)
        {
            // Get unique name, for example: 04256520_c856e6b37c9e12ab8a3de2846876a3c7
            var pathArr0 = path.Split('/');
            var catId = pathArr0[0];
            var cadId = pathArr0[1];
            var goName = catId + "_" + cadId;
            print(goName);

            var path_segs = path.Split('.');
            if (vertex_colors != null && colorModels)
            {
                int pointCount = vertex_colors.Length / 6; // Assuming each point has XYZRGB
                vertexPoints = new Vector3[pointCount];
                vertexColors = new Color[pointCount];
                ParseFloatArrayToColorArray(vertex_colors, vertexColors, vertexPoints);
            }
            else
            {
                vertexColors = new Color[0];
                vertexPoints = new Vector3[0];
            }
            path_segs[path_segs.Length - 1] = "obj";
            // offPath = Path.Join(databasePath, String.Join('.', path_segs));

            tMat = ConvertArrayTo4x4Matrix(t_mat);

            // SceneParent = gameObject.transform.Find("CADObjScene");
            GameObject scene = GameObject.FindGameObjectWithTag("Scene");

            try
            {
                Debug.Log("Loading file: " + goName.Split('_')[1] + ".prefab");
                if (go != null)
                    Destroy(go);

                var goPrefab = PhotonNetwork.Instantiate(databasePath + goName.Split('_')[0] + "/" + goName.Split('_')[1] + "/" + goName.Split('_')[1], new Vector3(0, 0, 0), Quaternion.identity);

                if (!colorModels)
                {
                    ApplyTransform(goPrefab, tMat);
                }
                else if (vertexColors.Length > 0)
                {
                    ApplyTransform(goPrefab, tMat, vertexPoints, vertexColors);
                }
                else
                {
                    ApplyTransform(goPrefab, tMat, color);
                }

                goPrefab.transform.SetParent(scene.transform);
            }
            catch (Exception e)
            {
                Debug.Log("File doesn't exist: " + goName.Split('_')[1] + ".prefab, " + "error: " + e.Message);
            }
        }

        public override string ToString()
        {
            return "\tpath: " + path;
        }
    }

    [Serializable]
    public class ReconLayout
    {
        public Color color = new Color();
        public GameObject obj;
        public float[] t_mat;
        public Matrix4x4 tMat;
        
        Matrix4x4 BuildTransformationMatrix(float[] rotationMatrix, Vector3 scale, Vector3 t)
        {
            // Assuming rotationMatrix is a 1D float array representing a 3x3 rotation matrix
            Matrix4x4 transformationMatrix = Matrix4x4.identity;

            // Set the upper-left 3x3 of the transformation matrix to the rotation matrix
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    transformationMatrix[i, j] = rotationMatrix[i * 3 + j];
                }
            }

            // Set the scale
            transformationMatrix.m00 *= scale.x;
            transformationMatrix.m11 *= scale.y;
            transformationMatrix.m22 *= scale.z;

            // Set the translation
            transformationMatrix.m03 = t.x;
            transformationMatrix.m13 = t.y;
            transformationMatrix.m23 = t.z;

            return transformationMatrix;
        }
        public void Load(GameObject parent)
        {
            obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.transform.SetParent(parent.transform, worldPositionStays:true);
            tMat = ConvertArrayTo4x4Matrix(t_mat);
            ApplyTransform(obj, tMat, color);
            Debug.Log("Loaded structure: " + ToString());
        }
        public override string ToString()
        {
            return "Layout object.";
        }
    }

    [Serializable]
    public class ReconScene
    {
        public List<ReconObject> objects = new List<ReconObject>();
        public List<ReconLayout> structures = new List<ReconLayout>();
        public List<bool> layoutMask;
        public void Load(bool loadLayoutPcd, bool colorModels, string databasePath)
        {
            GameObject scene = GameObject.FindGameObjectWithTag("Scene");
            scene.transform.position = Vector3.zero;
            scene.transform.localScale = Vector3.one;

            for (int i = 0; i < scene.transform.childCount; i++)
            {
                PhotonNetwork.Destroy(scene.transform.GetChild(i).GetComponent<PhotonView>());
            }
            Debug.Log("Old CAD Objects deleted");

            // SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

            // Load all objects
            foreach (var obj in objects)
            {
                obj.Load(scene, colorModels, databasePath);
            }
            
            if(!loadLayoutPcd)
            {
                // Load all structures
                foreach (var structure in structures)
                {
                    structure.Load(scene);
                }
            }
            
            // // Reposition light source
            // var light = GameObject.Find("Light");
            Bounds worldBounds = FindBounds();
            // Debug.Log("Found world bounds: " + worldBounds);
            // light.transform.position = worldBounds.center;
            
            // Reposition camera
            //var camera = GameObject.Find("Main Camera");
            //camera.transform.position = worldBounds.center-worldBounds.extents;
            //camera.transform.LookAt(Vector3.down);
            // camera.transform.LookAt(scene.transform);
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
    public static bool loadLayoutPcd;
    public static bool colorModels;
    // public string databasePath;
    public bool shapenetModel = true;
    internal Dictionary<string, Material> Materials;
    
    private static ReconScene scene;
    private ReconObject sampleObj;

    private static GameObject layoutGO;
    // Start is called before the first frame update
    void Start()
    {
        loadLayoutPcd = false;
        colorModels = false;
    }

    public static void JsonSceneProcessor(string jsonString, string databasePath)
    {
        var t_start = System.DateTime.Now;

        scene = new ReconScene();
        scene = JsonUtility.FromJson<ReconScene>(jsonString);

        scene.Load(loadLayoutPcd, colorModels, databasePath);
        var time_taken = System.DateTime.Now - t_start;
        // Debug.Log($"Time taken = {time_taken}");
    }

    public static Matrix4x4 ConvertArrayTo4x4Matrix(float[] array)
    {
        Matrix4x4 matrix = new Matrix4x4();

        for (int i = 0; i < 16; i++)
        {
            matrix[i / 4, i % 4] = array[i];
        }
        return matrix;
    }
    public static void ParseFloatArrayToColorArray(float[] flattenedArray, Color[] vertexColors, Vector3[] vertexPoints)
    {
        for (int i = 0; i < vertexPoints.Length; i++)
        {
            // Extract XYZ data
            float x = flattenedArray[i * 6];
            float y = flattenedArray[i * 6 + 1];
            float z = flattenedArray[i * 6 + 2];
            vertexPoints[i] = new Vector3(x, y, z);

            // Extract RGB data
            float r = flattenedArray[i * 6 + 3]; // Normalize to 0-1
            float g = flattenedArray[i * 6 + 4];
            float b = flattenedArray[i * 6 + 5];
            vertexColors[i] = new Color(r, g, b);
        }
    }
    public static Quaternion ExtractRotation(Matrix4x4 matrix)
    {
        Vector3 forward;
        forward.x = matrix.m02;
        forward.y = matrix.m12;
        forward.z = matrix.m22;
 
        Vector3 upwards;
        upwards.x = matrix.m01;
        upwards.y = matrix.m11;
        upwards.z = matrix.m21;
 
        return Quaternion.LookRotation(forward, upwards);
    }
    public static Vector3 ExtractPosition(Matrix4x4 matrix)
    {
        Vector3 position;
        position.x = matrix.m03;
        position.y = matrix.m13;
        position.z = matrix.m23;
        return position;
    }
    public static Vector3 ExtractScale(Matrix4x4 matrix)
    {
        Vector3 scale;
        scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
        scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
        scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
        return scale;
    }
    public static void Apply4x4Matrix(Transform transform, Matrix4x4 matrix)
    {
        transform.localScale = ExtractScale(matrix);
        transform.rotation = ExtractRotation(matrix);
        transform.position += ExtractPosition(matrix);
    }
    public static void ApplyTransform(GameObject obj, Matrix4x4 tMat, Color color)
    {
        Apply4x4Matrix(obj.transform, tMat);
        var meshRenderers = obj.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            for(int j=0; j<meshRenderers[i].materials.Length; j++)
            {
                meshRenderers[i].materials[j].color = color;
                // meshRenderers[i].materials[j].shader = Shader.Find("Point Cloud/Disk");
                meshRenderers[i].materials[j].shader = Shader.Find("Standard");
            }
        }
    }
    public static void ApplyTransform(GameObject obj, Matrix4x4 tMat)
    {
        Apply4x4Matrix(obj.transform, tMat);
    }
    public static void ApplyTransform(GameObject obj, Matrix4x4 tMat, Vector3[] vertexPoints, Color[] vertexColors)
    {
        Apply4x4Matrix(obj.transform, tMat);
        var meshRenderers = obj.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            Vector3 position = meshRenderers[i].bounds.center;
            int closestPointIndex = FindClosestPointIndex(vertexPoints, position);
            Color color = vertexColors[closestPointIndex];
            for(int j=0; j<meshRenderers[i].materials.Length; j++)
            {
                meshRenderers[i].materials[j].color = color;
                // meshRenderers[i].materials[j].shader = Shader.Find("Point Cloud/Disk");
                meshRenderers[i].materials[j].shader = Shader.Find("Particles/Standard Surface");
            }
        }
    }
    public static int FindClosestPointIndex(Vector3[] arr, Vector3 targetPoint)
    {
        if (arr == null || arr.Length == 0)
        {
            Debug.LogError("Vector3 array is null or empty.");
            return -1; // or some default value indicating no valid index
        }

        int closestIndex = 0; // Start with the index of the first element
        float closestDistance = Vector3.Distance(targetPoint, arr[0]);

        for (int i = 1; i < arr.Length; i++)
        {
            float distance = Vector3.Distance(targetPoint, arr[i]);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    public static Mesh DeleteVerticesFromMesh(Mesh mesh, List<bool> vertexMask)
    {
        // Retrieve original mesh data
        Vector3[] originalVertices = mesh.vertices;
        int[] originalTriangles = mesh.triangles;

        // Initialize lists for new mesh data
        System.Collections.Generic.List<Vector3> newVertices = new System.Collections.Generic.List<Vector3>();
        System.Collections.Generic.List<int> newTriangles = new System.Collections.Generic.List<int>();

        // Iterate through the vertices and triangles, excluding those indicated by the mask
        for (int i = 0; i < originalVertices.Length; i++)
        {
            if (!vertexMask[i]) // If the vertex is not marked for removal
            {
                // Add the vertex to the new vertices list
                newVertices.Add(originalVertices[i]);
            }
        }

        // Update triangle indices to reflect changes in the vertices array
        for (int i = 0; i < originalTriangles.Length; i += 3)
        {
            int vertexIndex1 = originalTriangles[i];
            int vertexIndex2 = originalTriangles[i + 1];
            int vertexIndex3 = originalTriangles[i + 2];

            if (!vertexMask[vertexIndex1] && !vertexMask[vertexIndex2] && !vertexMask[vertexIndex3])
            {
                // Add the triangle to the new triangles list
                newTriangles.Add(newVertices.IndexOf(originalVertices[vertexIndex1]));
                newTriangles.Add(newVertices.IndexOf(originalVertices[vertexIndex2]));
                newTriangles.Add(newVertices.IndexOf(originalVertices[vertexIndex3]));
            }
        }

        // Create a new mesh with the updated data
        Mesh newMesh = new Mesh();
        newMesh.SetVertices(newVertices);
        newMesh.SetTriangles(newTriangles.ToArray(), 0);

        // Update the mesh filter with the new mesh
        return mesh;
    }

    public static string[] GetParentFolders(string filePath)
    {
        // Get the directory name of the file path
        string directory = Path.GetDirectoryName(filePath);

        // Initialize a list to store parent folders
        System.Collections.Generic.List<string> parentFolders = new System.Collections.Generic.List<string>();

        while (!string.IsNullOrEmpty(directory) && directory != Path.GetPathRoot(directory))
        {
            parentFolders.Add(directory);
            directory = Path.GetDirectoryName(directory);
        }

        // Add the root directory if needed
        if (!string.IsNullOrEmpty(directory))
        {
            parentFolders.Add(directory);
        }

        // Reverse the list to get the parent folders in the correct order
        parentFolders.Reverse();

        return parentFolders.ToArray();
    }

    public static string[] GetOBJFileNames(string directory)
    {
        List<string> objFiles = new List<string>();

        // Check if the directory exists
        if (Directory.Exists(directory))
        {
            try
            {
                // Get all .obj file paths in the current directory
                string[] filePaths = Directory.GetFiles(directory, "*.obj");

                // Add the file names to the list
                foreach (string filePath in filePaths)
                {
                    objFiles.Add(filePath);
                }

                // Get all subdirectories
                string[] subDirectories = Directory.GetDirectories(directory);

                // Recursively search subdirectories for .obj files
                foreach (string subDir in subDirectories)
                {
                    string[] subDirFiles = GetOBJFileNames(subDir);
                    objFiles.AddRange(subDirFiles);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error searching directory: " + ex.Message);
            }
        }
        else
        {
            Debug.LogError("Directory does not exist: " + directory);
        }

        return objFiles.ToArray();
    }


    // Update is called once per frame
    void Update()
    {

    }
}
