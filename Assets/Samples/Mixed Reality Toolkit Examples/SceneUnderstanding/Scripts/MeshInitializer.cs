using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshInitializer : MonoBehaviour
{
    private Mesh receivedMesh;
    public GameObject meshTestObject;
    public static int meshTestFlag;

    public Material meshMaterial;

    // Start is called before the first frame update
    void Start()
    {
        receivedMesh = new Mesh();
        receivedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        // meshTestObject = new GameObject("MeshTesting", typeof(MeshFilter), typeof(MeshRenderer));
        // meshTestObject.transform.localScale = new Vector3(1, 1, 1);
        meshTestFlag = -1;
    }

    // Update is called once per frame
    void Update()
    {
        if (meshTestFlag == 0)
        {
            //if (Microsoft.MixedReality.Toolkit.Experimental.SceneUnderstanding.DemoSceneUnderstandingController.unityVertices.Length != 0)
            if (TCPTestClient.InTriVec.Length != 0)
            {
                receivedMesh.Clear();
                receivedMesh.vertices = TCPTestClient.InVerVec;
                receivedMesh.triangles = TCPTestClient.InTriVec;
                // receivedMesh.vertices = Microsoft.MixedReality.Toolkit.Experimental.SceneUnderstanding.DemoSceneUnderstandingController.unityVertices;
                // receivedMesh.triangles = Microsoft.MixedReality.Toolkit.Experimental.SceneUnderstanding.DemoSceneUnderstandingController.unityMeshTriangles;
                receivedMesh.RecalculateBounds();
                receivedMesh.RecalculateNormals();
                meshTestObject.GetComponent<MeshFilter>().mesh = receivedMesh;
                meshTestObject.GetComponent<MeshRenderer>().material = meshMaterial;
                meshTestObject.transform.localScale = new Vector3(1, 1, 1);
                meshTestObject.transform.position = TCPTestClient.positionVector;
                meshTestObject.transform.rotation = TCPTestClient.rotationVector;

                meshTestFlag = 1;
            }
        }
    }
}
