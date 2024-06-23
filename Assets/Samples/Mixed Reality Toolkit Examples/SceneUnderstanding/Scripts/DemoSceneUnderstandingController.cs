// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit.Examples.Demos;
using Microsoft.MixedReality.Toolkit.Experimental.SpatialAwareness;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Rendering;
using System.Runtime.InteropServices;
using Unity.Netcode;
using Photon.Pun;
using Photon.Realtime;
using MRTK.Tutorials.MultiUserCapabilities;
using UnityEditor;
using System.Collections;
using Microsoft.MixedReality.Toolkit.WindowsSceneUnderstanding.Experimental;

#if WINDOWS_UWP
using Windows.Storage;
#endif


namespace Microsoft.MixedReality.Toolkit.Experimental.SceneUnderstanding
{
    /// <summary>
    /// Demo class to show different ways of visualizing the space using scene understanding.
    /// </summary>
    public class DemoSceneUnderstandingController : DemoSpatialMeshHandler, IMixedRealitySpatialAwarenessObservationHandler<SpatialAwarenessSceneObject>
    {

        #region Private Fields

        #region Serialized Fields

        [SerializeField]
        private string SavedSceneNamePrefix = "DemoSceneUnderstanding";
        [SerializeField]
        private bool InstantiatePrefabs = false;
        [SerializeField]
        private GameObject InstantiatedPrefab = null;
        [SerializeField]
        private Transform InstantiatedParent = null;

        [Header("UI")]
        [SerializeField]
        private Interactable autoUpdateToggle = null;
        [SerializeField]
        private Interactable quadsToggle = null;
        [SerializeField]
        private Interactable inferRegionsToggle = null;
        [SerializeField]
        private Interactable meshesToggle = null;
        [SerializeField]
        private Interactable maskToggle = null;
        [SerializeField]
        private Interactable platformToggle = null;
        [SerializeField]
        private Interactable wallToggle = null;
        [SerializeField]
        private Interactable floorToggle = null;
        [SerializeField]
        private Interactable ceilingToggle = null;
        [SerializeField]
        private Interactable worldToggle = null;
        [SerializeField]
        private Interactable completelyInferred = null;
        [SerializeField]
        private Interactable backgroundToggle = null;

        [Header("WallOptions")]
        [SerializeField]
        private Interactable NearWallOption = null;
        [SerializeField]
        private Interactable MiddleWallOption = null;
        [SerializeField]
        private Interactable FarWallOption = null;
        [SerializeField]
        private Interactable EnableWallOption = null;
        [SerializeField]
        private Interactable EnableTransparent = null;
        [SerializeField]
        private Interactable UpdateWallList = null;
        [SerializeField]
        private Interactable ClippingPlaneOption = null;
        [SerializeField]
        private Interactable XRayVisionEyeGazeOption = null;

        [Header("Adaptive X-ray vision")]
        [SerializeField]
        private Interactable AdaptiveDistance = null;
        [SerializeField]
        private GameObject MessageTransformer = null;

        #endregion Serialized Fields

        private IMixedRealitySceneUnderstandingObserver observer;

        private List<GameObject> instantiatedPrefabs;

        private Dictionary<SpatialAwarenessSurfaceTypes, Dictionary<int, SpatialAwarenessSceneObject>> observedSceneObjects;

        private Dictionary<int, GameObject> wallIdPrefabDictionary;

        private List<GameObject> nearWall = new List<GameObject>(), middleWall = new List<GameObject>(), farWall = new List<GameObject>();

        private Vector3 previousCameraPosition;

        private int wallFlag = 1;

        private int transWallFlag = 0;

        private Color colorOrig;

        private GameObject clippingBox;

        private int wallTransEyeGazeFlag = 0;

        public static int photoCaptureFlag = 0;

        private int timeFlag = 0;

        private DateTime XRayTime;

        private Mesh unityStyleMesh;

        private object[] meshObjectArray;

        private GameObject TCPServerGameObject;

        private Dictionary<int, int> PhotonObjectIDLookUp;

        #endregion Private Fields

        #region MonoBehaviour Functions

        public Material wallTransMaterial;

        public Material gridGroundMaterial; // Change ground material to grid like at runtime

        public ClippingPrimitive addMesh2ClippingList; // Add all wall objects to become clip-able using the clipping box

        public AudioClip WallTransparentAudio; // Used as a sound effect to indicate wall objects become transparent

        public PhotonView _photonView;

        protected override void Start()
        {
            observer = CoreServices.GetSpatialAwarenessSystemDataProvider<IMixedRealitySceneUnderstandingObserver>();

            if (observer == null)
            {
                Debug.LogError("Couldn't access Scene Understanding Observer! Please make sure the current build target is set to Universal Windows Platform. "
                    + "Visit https://docs.microsoft.com/windows/mixed-reality/mrtk-unity/features/spatial-awareness/scene-understanding for more information.");
                return;
            }
            InitToggleButtonState();
            instantiatedPrefabs = new List<GameObject>();
            observedSceneObjects = new Dictionary<SpatialAwarenessSurfaceTypes, Dictionary<int, SpatialAwarenessSceneObject>>(); // <<< Here is the dictionary we need
            wallIdPrefabDictionary = new Dictionary<int, GameObject>(); // Create a gameobject dictionary to save all observed wall objects
            PhotonObjectIDLookUp = new Dictionary<int, int>(); // Create a dictionary for the program to find photon object's ViewID by using scene object ID
            previousCameraPosition = Camera.main.transform.position;
            // UpdateWallListByDistance();
            clippingBox = GameObject.Find("ClippingBox");
            // Update wall list and texture after 2 seconds from start
            Invoke("UpdateWallListByDistance", 2f);

            PhotonRoom.sceneUnderstandingObject = InstantiatedPrefab;
            PhotonRoom.MessageTransformerObject = MessageTransformer;

            meshObjectArray = new object[30000];

        }


        void Update()
        {
            // Do update scene objects when users' movement larger than 1 meter
            if (Vector3.Distance(previousCameraPosition, Camera.main.transform.position) > 1)
            {
                previousCameraPosition = Camera.main.transform.position;
                UpdateWallListByDistance();

                // print(WindowsSceneUnderstandingObserver.correctOrientation);
                // print(WindowsSceneUnderstandingObserver.sceneToWorldTransformMatrix);
            }
            if (wallTransEyeGazeFlag == 1)
            {
                Vector3 container = CoreServices.InputSystem.EyeGazeProvider.HitPosition;
                //if (Vector3.Distance(container, clippingBox.transform.position) > 1)
                //{
                //    DateTime dt = DateTime.Now;
                //    int ms = dt.Millisecond;
                //    print("X-Ray moving start");
                //    print(dt.ToString());
                //    print(ms.ToString());
                //    timeFlag = 2;
                //}
                if (XRayMovingTimer.xRayTimerFlag == 1)
                {
                    XRayTime = DateTime.Now;
                }
                if (timeFlag == 0)
                {
                    DateTime dt = DateTime.Now;
                    int ms = dt.Millisecond;
                    // print("X-Ray showing start");
                    // print(dt.ToString());
                    // print(ms.ToString());
                }
                clippingBox.transform.position = container;
                if (timeFlag == 0)
                {
                    DateTime dt = DateTime.Now;
                    int ms = dt.Millisecond;
                    // print("X-Ray showing end");
                    // print(dt.ToString());
                    // print(ms.ToString());
                }
                //if (timeFlag == 2)
                //{
                //    DateTime dt = DateTime.Now;
                //    int ms = dt.Millisecond;
                //    print("X-Ray moving end");
                //    print(dt.ToString());
                //    print(ms.ToString());
                //}
                timeFlag = 1;
                if (XRayMovingTimer.xRayTimerFlag == 1)
                {
                    if (XRayTime != DateTime.Now)
                        print(XRayTime.Subtract(DateTime.Now));
                }
            }
        }

        protected override void OnEnable()
        {
            RegisterEventHandlers<IMixedRealitySpatialAwarenessObservationHandler<SpatialAwarenessSceneObject>, SpatialAwarenessSceneObject>();
        }

        protected override void OnDisable()
        {
            UnregisterEventHandlers<IMixedRealitySpatialAwarenessObservationHandler<SpatialAwarenessSceneObject>, SpatialAwarenessSceneObject>();
        }

        protected override void OnDestroy()
        {
            UnregisterEventHandlers<IMixedRealitySpatialAwarenessObservationHandler<SpatialAwarenessSceneObject>, SpatialAwarenessSceneObject>();
        }

        #endregion MonoBehaviour Functions

        #region IMixedRealitySpatialAwarenessObservationHandler Implementations

        public Material ShowerBehindTheWall;

        public static Vector3[] unityVertices;
        public static Int32[] unityMeshTriangles;

        /// <inheritdoc />
        public void OnObservationAdded(MixedRealitySpatialAwarenessEventData<SpatialAwarenessSceneObject> eventData)
        {
            // This method called everytime a SceneObject created by the SU observer
            // The eventData contains everything you need do something useful

            AddToData(eventData.Id);

            if (eventData.SpatialObject.SurfaceType == SpatialAwarenessSurfaceTypes.World)
            {
                unityStyleMesh = UnityMeshFromMeshData(eventData.SpatialObject.Meshes[0]);
                unityVertices = unityStyleMesh.vertices;
                // print(unityVertices.Length);
                unityMeshTriangles = unityStyleMesh.triangles;
                // print(unityVertices.Length);
                // print(unityMeshTriangles.Length);
                //PhotonHandler.MaxDatagrams = 10000;
                //print(PhotonHandler.MaxDatagrams);
                //for (int i = 0; i <= unityVertices.Length; i += 30001)
                //{
                //    print(meshObjectArray.Length);
                //    if (unityVertices.Length - i < 30000)
                //    {
                //        Array.Copy(unityVertices, i, meshObjectArray, 0, unityVertices.Length - i);
                //    }
                //    else
                //    {
                //        Array.Copy(unityVertices, i, meshObjectArray, 0, 30000);
                //    }
                //    var MesaageTransformer = PhotonNetwork.Instantiate(PhotonRoom.MessageTransformerObject.name, new Vector3(0, 0, 0), Quaternion.identity, 0, meshObjectArray);
                //    Array.Clear(meshObjectArray, 0, meshObjectArray.Length);
                //}
                // var MesaageTransformer = PhotonNetwork.Instantiate(PhotonRoom.MessageTransformerObject.name, new Vector3(0, 0, 0), Quaternion.identity, 0, JSONReader.JsonDataArray);
                // Array.Copy(unityVertices, 0, meshObjectArray, 0, 30000);
                // var MesaageTransformer = PhotonNetwork.Instantiate(PhotonRoom.MessageTransformerObject.name, new Vector3(0, 0, 0), Quaternion.identity, 0, meshObjectArray);
                // print("Oh it works!");
                // _photonView.RpcSecure("RPC_WorldMeshSystem", RpcTarget.All, true, meshObjectArray);
                FileNameInitialize.writer.WriteLine(" ");
                FileNameInitialize.writer.WriteLine("--------- Process Start Time: " + DateTime.Now.ToString("h:mm:ss.ffffff tt") + " -----------------");
                TCPServerGameObject = GameObject.Find("TestConnection");
                if (TCPServerGameObject.GetComponent<TCPTestServer>().enabled == true)
                    TCPServerGameObject.GetComponent<TCPTestServer>().VecToByteConverter();
                FileNameInitialize.writer.WriteLine("--------- Process Ended Time: " + DateTime.Now.ToString("h:mm:ss.ffffff tt") + " -----------------");
                FileNameInitialize.writer.WriteLine(" ");
                Debug.Log("Finish CAMRE-CAD Processing");
            }

            // float dist = Vector3.Distance(eventData.SpatialObject.Position, Camera.main.transform.position);

            // If the eventData surface type did not create in the dictionary, create one new key with a new dictionary
            if (observedSceneObjects.TryGetValue(eventData.SpatialObject.SurfaceType, out Dictionary<int, SpatialAwarenessSceneObject> sceneObjectDict))
            {
                sceneObjectDict.Add(eventData.Id, eventData.SpatialObject);
            }
            else
            {
                observedSceneObjects.Add(eventData.SpatialObject.SurfaceType, new Dictionary<int, SpatialAwarenessSceneObject> { { eventData.Id, eventData.SpatialObject } });
            }

            if (InstantiatePrefabs && eventData.SpatialObject.Quads.Count > 0) // If prefabs option is enable and quad number in this eventData is more than zero, create prefabs
            {
                // InstantiatedPrefab
                // var prefab = PhotonNetwork.Instantiate(PhotonRoom.sceneUnderstandingObject.name, eventData.SpatialObject.Position, eventData.SpatialObject.Rotation);

                //var prefab = Instantiate(InstantiatedPrefab);
                //prefab.GetComponent<NetworkObject>().Spawn(true);
                //prefab.transform.SetPositionAndRotation(eventData.SpatialObject.Position, eventData.SpatialObject.Rotation);

                var prefab = PhotonNetwork.Instantiate(PhotonRoom.sceneUnderstandingObject.name, eventData.SpatialObject.Position, eventData.SpatialObject.Rotation);
                // prefab.name = eventData.SpatialObject.SurfaceType.ToString() + ' ' + eventData.Id.ToString();
                // print(prefab.GetPhotonView());
                PhotonObjectIDLookUp[eventData.Id] = prefab.GetComponent<PhotonView>().ViewID;
                if (InstantiatedParent)
                {
                    switch (eventData.SpatialObject.SurfaceType)
                    {
                        case SpatialAwarenessSurfaceTypes.Floor:
                            prefab.transform.SetParent(InstantiatedParent.GetChild(0));
                            break;
                        case SpatialAwarenessSurfaceTypes.Ceiling:
                            prefab.transform.SetParent(InstantiatedParent.GetChild(1));
                            break;
                        case SpatialAwarenessSurfaceTypes.Wall:
                            prefab.transform.SetParent(InstantiatedParent.GetChild(2));
                            break;
                        case SpatialAwarenessSurfaceTypes.Platform:
                            prefab.transform.SetParent(InstantiatedParent.GetChild(3));
                            break;
                        case SpatialAwarenessSurfaceTypes.Background:
                            prefab.transform.SetParent(InstantiatedParent.GetChild(4));
                            break;
                        case SpatialAwarenessSurfaceTypes.Inferred:
                            prefab.transform.SetParent(InstantiatedParent.GetChild(5));
                            break;
                        case SpatialAwarenessSurfaceTypes.World:
                            prefab.transform.SetParent(InstantiatedParent.GetChild(6));
                            break;
                        default:
                            prefab.transform.SetParent(InstantiatedParent);
                            break;
                    }
                }
                instantiatedPrefabs.Add(prefab);

                float sx = eventData.SpatialObject.Quads[0].Extents.x;
                float sy = eventData.SpatialObject.Quads[0].Extents.y;
                prefab.transform.localScale = new Vector3(sx, sy, .1f);
                //// Change object prefabs' texture to fix the stretched textures
                float gridTileScale = 4;
                Vector2 quadTextureTiling = new Vector2(prefab.transform.localScale.x, prefab.transform.localScale.y);
                prefab.GetComponentsInChildren<Renderer>()[0].material.mainTextureScale = quadTextureTiling / gridTileScale;
                
                prefab.GetComponentsInChildren<Renderer>()[0].material.color = ColorForSurfaceType(eventData.SpatialObject.SurfaceType);
                if (eventData.SpatialObject.SurfaceType == SpatialAwarenessSurfaceTypes.Wall)
                {
                    addMesh2ClippingList.AddRenderer(prefab.GetComponentsInChildren<MeshRenderer>()[0]);
                    wallIdPrefabDictionary[eventData.Id] = prefab;
                }
                foreach (var quad in eventData.SpatialObject.Quads)
                {
                    quad.GameObject.GetComponent<Renderer>().material.color = ColorForSurfaceType(eventData.SpatialObject.SurfaceType); // Put color on the renderer to avoid confusion
                }

                // PhotonNetwork.Destroy(PhotonView.Find(prefab.GetComponent<PhotonView>().ViewID));

                // PhotonNetwork.Destroy(GameObject.Find(eventData.SpatialObject.SurfaceType.ToString() + ' ' + eventData.Id.ToString()));

                //float sx = eventData.SpatialObject.Quads[0].Extents.x;
                //float sy = eventData.SpatialObject.Quads[0].Extents.y;
                //InstantiatedPrefab.transform.localScale = new Vector3(sx, sy, .1f);
                //float gridTileScale = 4;
                //Vector2 quadTextureTiling = new Vector2(InstantiatedPrefab.transform.localScale.x, InstantiatedPrefab.transform.localScale.y);
                // InstantiatedPrefab.GetComponentsInChildren<Renderer>()[0].sharedMaterial.color = ColorForSurfaceType(eventData.SpatialObject.SurfaceType);
                //foreach (var quad in eventData.SpatialObject.Quads)
                //{
                //    quad.GameObject.GetComponent<Renderer>().material.color = ColorForSurfaceType(eventData.SpatialObject.SurfaceType); // Put color on the renderer to avoid confusion
                //}
                //var prefab = PhotonNetwork.Instantiate(PhotonRoom.sceneUnderstandingObject.name, eventData.SpatialObject.Position, eventData.SpatialObject.Rotation);
            }
            else
            {
                foreach (var quad in eventData.SpatialObject.Quads)
                {
                    if (eventData.SpatialObject.SurfaceType == SpatialAwarenessSurfaceTypes.Floor) // Add grid material
                    {
                        quad.GameObject.GetComponent<Renderer>().material = gridGroundMaterial;
                        quad.GameObject.GetComponent<Renderer>().material.mainTextureScale = new Vector2(quad.GameObject.transform.localScale.x / quad.GameObject.transform.localScale.y, 1);
                    }
                    quad.GameObject.GetComponent<Renderer>().material.color = ColorForSurfaceType(eventData.SpatialObject.SurfaceType); // If prefab option is disable, put color on the renderer
                    //if (eventData.SpatialObject.SurfaceType == SpatialAwarenessSurfaceTypes.Platform) // Add new texture on the platform objects
                    //{
                    //    quad.GameObject.GetComponent<Renderer>().material = ShowerBehindTheWall;
                    //}
                    // Change objects' texture to fix the stretched textures
                    float gridTileScale = 4;
                    Vector2 quadTextureTiling = new Vector2(quad.GameObject.transform.localScale.x, quad.GameObject.transform.localScale.y);
                    quad.GameObject.GetComponent<Renderer>().material.mainTextureScale = quadTextureTiling / gridTileScale;
                }

            }
        }

        /// <inheritdoc />
        public void OnObservationUpdated(MixedRealitySpatialAwarenessEventData<SpatialAwarenessSceneObject> eventData)
        {
            UpdateData(eventData.Id);

            if (observedSceneObjects.TryGetValue(eventData.SpatialObject.SurfaceType, out Dictionary<int, SpatialAwarenessSceneObject> sceneObjectDict))
            {
                observedSceneObjects[eventData.SpatialObject.SurfaceType][eventData.Id] = eventData.SpatialObject;
            }
            else
            {
                observedSceneObjects.Add(eventData.SpatialObject.SurfaceType, new Dictionary<int, SpatialAwarenessSceneObject> { { eventData.Id, eventData.SpatialObject } });
            }
        }

        /// <inheritdoc />
        public void OnObservationRemoved(MixedRealitySpatialAwarenessEventData<SpatialAwarenessSceneObject> eventData)
        {
            RemoveFromData(eventData.Id);
            PhotonNetwork.Destroy(PhotonView.Find(PhotonObjectIDLookUp[eventData.Id]));

            foreach (var sceneObjectDict in observedSceneObjects.Values)
            {
                sceneObjectDict?.Remove(eventData.Id);
            }
        }

        #endregion IMixedRealitySpatialAwarenessObservationHandler Implementations

        #region CAARE Custom Functions

        public void UpdateWallListByDistance()
        {
            print("List update start");

            //DirectoryInfo dir = new DirectoryInfo(Application.streamingAssetsPath);
            //DirectoryInfo[] info = dir.GetDirectories();
            //foreach (var dirc in info)
            //{
            //    print(dirc);
            //}
            //print(File.Exists(Application.streamingAssetsPath + "\\Lab3.png"));

            // Separate wall objects into three different list, clean three lists before update start
            nearWall.Clear();
            middleWall.Clear();
            farWall.Clear();
            var wallDic = GetSceneObjectsOfType(SpatialAwarenessSurfaceTypes.Wall); // A dictionary to save all wall objects by using prebuilt function
            foreach (KeyValuePair<int, SpatialAwarenessSceneObject> wallPair in wallDic) // Each wall pair includes "Object's ID" as key and "Scene Object" as value
            {
                Renderer wallRenderer = wallPair.Value.GameObject.GetComponentsInChildren<Renderer>()[0];
                Vector2 quadTextureTiling = wallRenderer.material.mainTextureScale;
                wallRenderer.material = wallTransMaterial; // Change all wall objects into transparentable texture
                wallRenderer.material.SetTextureScale("_MainTex", quadTextureTiling);
                // wallPair.Value.GameObject.GetComponentsInChildren<Renderer>()[0].material = wallTransMaterial; // Change all wall objects into transparentable texture

                // Add Wall's mesh renderer into clip-able list
                addMesh2ClippingList.AddRenderer(wallPair.Value.GameObject.GetComponentInChildren<MeshRenderer>());
                // Calculate distance from user position to all wall objects' collider position
                float distWall = Vector3.Distance(wallPair.Value.GameObject.GetComponentInChildren<Collider>().ClosestPoint(Camera.main.transform.position), Camera.main.transform.position);
                if (distWall < 3)
                {
                    nearWall.Add(wallPair.Value.GameObject);
                    if (transWallFlag == 1) // If user toggles the wall objects transparent switch to on, do things in this if statement
                    {
                        AudioSource.PlayClipAtPoint(WallTransparentAudio, wallPair.Value.GameObject.GetComponentInChildren<Collider>().ClosestPoint(Camera.main.transform.position), 0.1f);
                        // Disable the script that deny the changing of wall objects (which is created by the clipping plane script)
                        wallPair.Value.GameObject.GetComponentsInChildren<MaterialInstance>()[0].enabled = false;
                        // Get near wall objects' color and make them 0.3 transparent
                        colorOrig = wallPair.Value.GameObject.GetComponentsInChildren<Renderer>()[0].material.color;
                        wallPair.Value.GameObject.GetComponentsInChildren<Renderer>()[0].material.color = new Color(colorOrig.r, colorOrig.g, colorOrig.b, 0.3f);

                        if (InstantiatePrefabs)
                        {
                            wallIdPrefabDictionary[wallPair.Key].GetComponentsInChildren<MaterialInstance>()[0].enabled = false;
                            wallIdPrefabDictionary[wallPair.Key].GetComponentsInChildren<Renderer>()[0].material.color = new Color(colorOrig.r, colorOrig.g, colorOrig.b, 0.3f);
                        }
                    }
                }
                else if (distWall >= 3 && distWall < 6) // Switch wall objects back to normal when users leave the wall objects
                {
                    wallPair.Value.GameObject.GetComponentsInChildren<MaterialInstance>()[0].enabled = true;
                    middleWall.Add(wallPair.Value.GameObject);
                    colorOrig = wallPair.Value.GameObject.GetComponentsInChildren<Renderer>()[0].material.color;
                    wallPair.Value.GameObject.GetComponentsInChildren<Renderer>()[0].material.color = new Color(colorOrig.r, colorOrig.g, colorOrig.b, 1f);

                    if (InstantiatePrefabs)
                        wallIdPrefabDictionary[wallPair.Key].GetComponentsInChildren<MaterialInstance>()[0].enabled = true;
                }
                else
                {
                    wallPair.Value.GameObject.GetComponentsInChildren<MaterialInstance>()[0].enabled = true;
                    farWall.Add(wallPair.Value.GameObject);
                    colorOrig = wallPair.Value.GameObject.GetComponentsInChildren<Renderer>()[0].material.color;
                    wallPair.Value.GameObject.GetComponentsInChildren<Renderer>()[0].material.color = new Color(colorOrig.r, colorOrig.g, colorOrig.b, 1f);

                    if (InstantiatePrefabs)
                        wallIdPrefabDictionary[wallPair.Key].GetComponentsInChildren<MaterialInstance>()[0].enabled = true;
                }
            }
        }

        private static Mesh UnityMeshFromMeshData(SpatialAwarenessSceneObject.MeshData meshData)
        {
            Mesh unityMesh = new Mesh();

            // Unity has a limit of 65,535 vertices in a mesh.
            // This limit exists because by default Unity uses 16-bit index buffers.
            // Starting with 2018.1, Unity allows one to use 32-bit index buffers.
            if (meshData.Vertices.Length > 65535)
            {
                unityMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            unityMesh.SetVertices(new List<Vector3>(meshData.Vertices));
            unityMesh.SetIndices(meshData.Indices, MeshTopology.Triangles, 0);
            unityMesh.SetUVs(0, new List<Vector2>(meshData.UVs));
            unityMesh.RecalculateNormals();

            return unityMesh;
        }

#endregion CAARE Custom Functions

#region Public Functions

        /// <summary>
        /// Get all currently observed SceneObjects of a certain type.
        /// </summary>
        /// <remarks>
        /// Before calling this function, the observer should be configured to observe the specified type by including that type in the SurfaceTypes property.
        /// </remarks>
        /// <returns>A dictionary with the scene objects of the requested type being the values and their ids being the keys.</returns>
        public IReadOnlyDictionary<int, SpatialAwarenessSceneObject> GetSceneObjectsOfType(SpatialAwarenessSurfaceTypes type)
        {
            if (!observer.SurfaceTypes.HasFlag(type))
            {
                Debug.LogErrorFormat("The Scene Objects of type {0} are not being observed. You should add {0} to the SurfaceTypes property of the observer in advance.", type);
            }

            if (observedSceneObjects.TryGetValue(type, out Dictionary<int, SpatialAwarenessSceneObject> sceneObjects))
            {
                return sceneObjects;
            }
            else
            {
                observedSceneObjects.Add(type, new Dictionary<int, SpatialAwarenessSceneObject>());
                return observedSceneObjects[type];
            }
        }

#region UI Functions

        /// <summary>
        /// Request the observer to update the scene
        /// </summary>
        public void UpdateScene()
        {
            // RemovePhotonObjects();
            FileNameInitialize.writer.WriteLine(" ");
            FileNameInitialize.writer.WriteLine("--------- Button Pressed at this Time: " + DateTime.Now.ToString("h:mm:ss.ffffff tt") + " -----------------");
            observer.UpdateOnDemand();
            UpdateWallListByDistance();
        }

        /// <summary>
        /// Request the observer to save the scene
        /// </summary>
        public void SaveScene()
        {
            observer.SaveScene(SavedSceneNamePrefix);
        }

        /// <summary>
        /// Request the observer to clear the observations in the scene
        /// </summary>
        public void ClearScene()
        {
            foreach (GameObject gameObject in instantiatedPrefabs)
            {
                Destroy(gameObject);
            }
            instantiatedPrefabs.Clear();
            observer.ClearObservations();
        }

        /// <summary>
        /// Change the auto update state of the observer
        /// </summary>
        public void ToggleAutoUpdate()
        {
            observer.AutoUpdate = !observer.AutoUpdate;
        }

        /// <summary>
        /// Change whether to request occlusion mask from the observer followed by
        /// clearing existing observations and requesting an update
        /// </summary>
        public void ToggleOcclusionMask()
        {
            var observerMask = observer.RequestOcclusionMask;
            observer.RequestOcclusionMask = !observerMask;
            if (observer.RequestOcclusionMask)
            {
                if (!(observer.RequestPlaneData || observer.RequestMeshData))
                {
                    observer.RequestPlaneData = true;
                    quadsToggle.IsToggled = true;
                }
            }
            ClearAndUpdateObserver();
        }

        /// <summary>
        /// Change whether to request plane data from the observer followed by
        /// clearing existing observations and requesting an update
        /// </summary>
        public void ToggleGeneratePlanes()
        {
            observer.RequestPlaneData = !observer.RequestPlaneData;
            if (observer.RequestPlaneData)
            {
                observer.RequestMeshData = false;
                meshesToggle.IsToggled = false;
            }
            ClearAndUpdateObserver();
        }

        /// <summary>
        /// Change whether to request mesh data from the observer followed by
        /// clearing existing observations and requesting an update
        /// </summary>
        public void ToggleGenerateMeshes()
        {
            observer.RequestMeshData = !observer.RequestMeshData;
            if (observer.RequestMeshData)
            {
                observer.RequestPlaneData = false;
                quadsToggle.IsToggled = false;
            }
            ClearAndUpdateObserver();
        }

        /// <summary>
        /// Change whether to request floor data from the observer followed by
        /// clearing existing observations and requesting an update
        /// </summary>
        public void ToggleFloors()
        {
            ToggleObservedSurfaceType(SpatialAwarenessSurfaceTypes.Floor);
            ClearAndUpdateObserver();
        }

        /// <summary>
        /// Change whether to request wall data from the observer followed by
        /// clearing existing observations and requesting an update
        /// </summary>
        // Change the original toggle wall function to new function
        public void ToggleWalls()
        {
            ToggleObservedSurfaceType(SpatialAwarenessSurfaceTypes.Wall);

            switch (wallFlag)
            {
                case 0:
                    foreach (GameObject wallObject in nearWall)
                    {
                        wallObject.GetComponentsInChildren<Renderer>()[0].enabled = true;
                    }
                    foreach (GameObject wallObject in middleWall)
                    {
                        wallObject.GetComponentsInChildren<Renderer>()[0].enabled = true;
                    }
                    foreach (GameObject wallObject in farWall)
                    {
                        wallObject.GetComponentsInChildren<Renderer>()[0].enabled = true;
                    }
                    wallFlag = 1;
                    print("Wall flag set to 1");
                    break;
                case 1:
                    foreach (GameObject wallObject in nearWall)
                    {
                        wallObject.GetComponentsInChildren<Renderer>()[0].enabled = false;
                    }
                    wallFlag = 2;
                    print("Wall flag set to 2");
                    break;
                case 2:
                    foreach (GameObject wallObject in middleWall)
                    {
                        wallObject.GetComponentsInChildren<Renderer>()[0].enabled = false;
                    }
                    wallFlag = 3;
                    print("Wall flag set to 3");
                    break;
                case 3:
                    foreach (GameObject wallObject in farWall)
                    {
                        print(wallObject);
                        wallObject.GetComponentsInChildren<Renderer>()[0].enabled = false;
                    }
                    wallFlag = 0;
                    print("Wall flag set to initial point");
                    break;
            }
            // ClearAndUpdateObserver();
        }

        /// <summary>
        /// Change whether to request ceiling data from the observer followed by
        /// clearing existing observations and requesting an update
        /// </summary>
        public void ToggleCeilings()
        {
            ToggleObservedSurfaceType(SpatialAwarenessSurfaceTypes.Ceiling);
            ClearAndUpdateObserver();
        }

        /// <summary>
        /// Change whether to request platform data from the observer followed by
        /// clearing existing observations and requesting an update
        /// </summary>
        public void TogglePlatforms()
        {
            ToggleObservedSurfaceType(SpatialAwarenessSurfaceTypes.Platform);
            ClearAndUpdateObserver();
        }

        /// <summary>
        /// Change whether to request inferred region data from the observer followed by
        /// clearing existing observations and requesting an update
        /// </summary>
        public void ToggleInferRegions()
        {
            observer.InferRegions = !observer.InferRegions;
            ClearAndUpdateObserver();
        }

        /// <summary>
        /// Change whether to request world mesh data from the observer followed by
        /// clearing existing observations and requesting an update
        /// </summary>
        public void ToggleWorld()
        {
            FileNameInitialize.writer.WriteLine("--------- Button Pressed at this Time: " + DateTime.Now.ToString("h:mm:ss.ffffff tt") + " -----------------");
            ToggleObservedSurfaceType(SpatialAwarenessSurfaceTypes.World);

            if (observer.SurfaceTypes.HasFlag(SpatialAwarenessSurfaceTypes.World))
            {
                // Ensure we requesting meshes
                observer.RequestMeshData = true;
                meshesToggle.GetComponent<Interactable>().IsToggled = true;
            }
            ClearAndUpdateObserver();
        }

        /// <summary>
        /// Change whether to request background data from the observer followed by
        /// clearing existing observations and requesting an update
        /// </summary>
        public void ToggleBackground()
        {
            ToggleObservedSurfaceType(SpatialAwarenessSurfaceTypes.Background);
            ClearAndUpdateObserver();
        }

        /// <summary>
        /// Change whether to request completely inferred data from the observer followed by
        /// clearing existing observations and requesting an update
        /// </summary>
        public void ToggleCompletelyInferred()
        {
            ToggleObservedSurfaceType(SpatialAwarenessSurfaceTypes.Inferred);
            ClearAndUpdateObserver();
        }

        public void ToggleNearWallOption()
        {
            foreach (GameObject wallObject in nearWall)
            {
                if (NearWallOption.IsToggled == true)
                    wallObject.GetComponentsInChildren<Renderer>()[0].enabled = true;
                else
                    wallObject.GetComponentsInChildren<Renderer>()[0].enabled = false;
            }
        }

        public void ToggleMiddleWallOption()
        {
            foreach (GameObject wallObject in middleWall)
            {
                if (MiddleWallOption.IsToggled == true)
                    wallObject.GetComponentsInChildren<Renderer>()[0].enabled = true;
                else
                    wallObject.GetComponentsInChildren<Renderer>()[0].enabled = false;
            }
        }

        public void ToggleFarWallOption()
        {
            foreach (GameObject wallObject in farWall)
            {
                if (FarWallOption.IsToggled == true)
                    wallObject.GetComponentsInChildren<Renderer>()[0].enabled = true;
                else
                    wallObject.GetComponentsInChildren<Renderer>()[0].enabled = false;
            }
        }

        public void PressableEnableAllWallOption()
        {
            foreach (GameObject wallObject in nearWall)
            {
                wallObject.GetComponentsInChildren<Renderer>()[0].enabled = true;
                NearWallOption.IsToggled = true;
            }
            foreach (GameObject wallObject in middleWall)
            {
                wallObject.GetComponentsInChildren<Renderer>()[0].enabled = true;
                MiddleWallOption.IsToggled = true;
            }
            foreach (GameObject wallObject in farWall)
            {
                wallObject.GetComponentsInChildren<Renderer>()[0].enabled = true;
                FarWallOption.IsToggled = true;
            }
        }

        public void ToggleWallTransparentOption()
        {
            if (transWallFlag == 0)
            {
                transWallFlag = 1;
                // clippingBox.GetComponent<Renderer>().enabled = false;
                ClippingPlaneOption.IsToggled = false;
                wallTransEyeGazeFlag = 0;
                clippingBox.transform.position = Camera.main.transform.position;
                clippingBox.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                clippingBox.GetComponent<ClippingBox>().enabled = false;
            }
            else
                transWallFlag = 0;
        }

        /*
         * Logical terms:
         * 1. When x-ray switch enable, eye-gaze switch enable --> x-ray window effect shows on eye-gaze hit points on the wall object (Disable collider, renderer)
         * 2. When x-ray switch enable, eye-gaze switch disable --> x-ray window shows in front of user and can be modifiable (Enable collider, renderer)
         * 3. When x-ray switch disable, eye-gaze switch enable --> Nothing happened, all appeared x-ray window effect disabled
         * 4. When x-ray switch disable, eye-gaze switch disable --> Nothing happened, all appeared x-ray window effect disabled
         * 5. When x-ray switch enable, wall transparent effect disabled
         */
        public void PressableUpdateWallListOption()
        {
            previousCameraPosition = Camera.main.transform.position;
            UpdateWallListByDistance();
        }

        public void ToggleClippingPlaneOption()
        {
            if (ClippingPlaneOption.IsToggled == true)
            {
                if (XRayVisionEyeGazeOption.IsToggled == true)
                {
                    // Make sure the clipping box is in front of the users' location, the size return to original size
                    clippingBox.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                    clippingBox.GetComponent<Renderer>().enabled = false;
                    // Disabling the collider to enable the eye gaze hit point function, which can avoid eye-gaze ray hit the x-ray window
                    clippingBox.GetComponent<Collider>().enabled = false;
                    // By setting the flag to 1, the eye gaze x-ray window is enabled
                    wallTransEyeGazeFlag = 1;
                    clippingBox.GetComponent<ClippingBox>().enabled = true;
                }
                else
                {
                    // Ensure we enable everything we needed, to show a moveable x-ray box for user to manually modify
                    wallTransEyeGazeFlag = 0;
                    clippingBox.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                    clippingBox.transform.position = CoreServices.InputSystem.EyeGazeProvider.GazeOrigin + CoreServices.InputSystem.EyeGazeProvider.GazeDirection.normalized * 1;
                    clippingBox.transform.eulerAngles = new Vector3(clippingBox.transform.eulerAngles.x, Camera.main.transform.eulerAngles.y - 90.0f, clippingBox.transform.eulerAngles.z);
                    clippingBox.GetComponent<Collider>().enabled = true;
                    clippingBox.GetComponent<Renderer>().enabled = true;
                    clippingBox.GetComponent<ClippingBox>().enabled = true;
                }
                EnableTransparent.IsToggled = false;
                transWallFlag = 0;
                // print(DateTime.Now.ToString("h:mm:ss.ffffff tt"));
            }
            else
            {
                // Disable all the x-ray function and make the x-ray box really small (in order to not present any effect) and at user's position
                wallTransEyeGazeFlag = 0;
                clippingBox.transform.position = Camera.main.transform.position;
                clippingBox.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                clippingBox.GetComponent<ClippingBox>().enabled = false;
                clippingBox.GetComponent<Renderer>().enabled = false;
                timeFlag = 0;
            }
        }

        public void ToggleXRayEyeGazeOption()
        {
            if (XRayVisionEyeGazeOption.IsToggled == true)
            {
                // If the x-ray option is not enabled, the eye gaze option will not do anything
                if (ClippingPlaneOption.IsToggled == true)
                {
                    clippingBox.GetComponent<Collider>().enabled = false;
                    wallTransEyeGazeFlag = 1;
                    clippingBox.GetComponent<Renderer>().enabled = false;
                }
            }
            else
            {
                if (ClippingPlaneOption.IsToggled == true)
                {
                    wallTransEyeGazeFlag = 0;
                    clippingBox.transform.position = CoreServices.InputSystem.EyeGazeProvider.GazeOrigin + CoreServices.InputSystem.EyeGazeProvider.GazeDirection.normalized * 1;
                    clippingBox.transform.eulerAngles = new Vector3(clippingBox.transform.eulerAngles.x, Camera.main.transform.eulerAngles.y - 90.0f, clippingBox.transform.eulerAngles.z);
                    clippingBox.GetComponent<Collider>().enabled = true;
                    clippingBox.GetComponent<Renderer>().enabled = true;
                }
            }
        }

        public void ToggleAdaptiveDistanceXRay()
        {
            if (photoCaptureFlag == 1)
            {
                clippingBox.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                photoCaptureFlag = 0;
            }
            else
                photoCaptureFlag = 1;
        }

        public void PressableSetDetailCoarse()
        {
            Debug.Log("Mesh Detail Set To Coarse");
            observer.WorldMeshLevelOfDetail = SpatialAwarenessMeshLevelOfDetail.Coarse;
        }

        public void PressableSetDetailMedium()
        {
            Debug.Log("Mesh Detail Set To Medium");
            observer.WorldMeshLevelOfDetail = SpatialAwarenessMeshLevelOfDetail.Medium;
        }

        public void PressableSetDetailFine()
        {
            Debug.Log("Mesh Detail Set To Fine");
            observer.WorldMeshLevelOfDetail = SpatialAwarenessMeshLevelOfDetail.Fine;
        }

        public void PressableSetDetailUnlimited()
        {
            Debug.Log("Mesh Detail Set To Unlimited");
            observer.WorldMeshLevelOfDetail = SpatialAwarenessMeshLevelOfDetail.Unlimited;
        }

        #endregion UI Functions

        #endregion Public Functions

        #region Helper Functions

        private void InitToggleButtonState()
        {
            // Configure observer
            autoUpdateToggle.IsToggled = observer.AutoUpdate;
            quadsToggle.IsToggled = observer.RequestPlaneData;
            meshesToggle.IsToggled = observer.RequestMeshData;
            maskToggle.IsToggled = observer.RequestOcclusionMask;
            inferRegionsToggle.IsToggled = observer.InferRegions;

            // Filter display
            platformToggle.IsToggled = observer.SurfaceTypes.HasFlag(SpatialAwarenessSurfaceTypes.Platform);
            wallToggle.IsToggled = observer.SurfaceTypes.HasFlag(SpatialAwarenessSurfaceTypes.Wall);
            floorToggle.IsToggled = observer.SurfaceTypes.HasFlag(SpatialAwarenessSurfaceTypes.Floor);
            ceilingToggle.IsToggled = observer.SurfaceTypes.HasFlag(SpatialAwarenessSurfaceTypes.Ceiling);
            worldToggle.IsToggled = observer.SurfaceTypes.HasFlag(SpatialAwarenessSurfaceTypes.World);
            completelyInferred.IsToggled = observer.SurfaceTypes.HasFlag(SpatialAwarenessSurfaceTypes.Inferred);
            backgroundToggle.IsToggled = observer.SurfaceTypes.HasFlag(SpatialAwarenessSurfaceTypes.Background);

            // Newly added button
            NearWallOption.IsToggled = true;
            MiddleWallOption.IsToggled = true;
            FarWallOption.IsToggled = true;
            EnableWallOption.IsToggled = false;
            EnableTransparent.IsToggled = false;
            UpdateWallList.IsToggled = false;
            ClippingPlaneOption.IsToggled = false;
            XRayVisionEyeGazeOption.IsToggled = true;

            AdaptiveDistance.IsToggled = false;
        }

        /// <summary>
        /// Gets the color of the given surface type
        /// </summary>
        /// <param name="surfaceType">The surface type to get color for</param>
        /// <returns>The color of the type</returns>
        private Color ColorForSurfaceType(SpatialAwarenessSurfaceTypes surfaceType)
        {
            // shout-out to solarized!

            switch (surfaceType)
            {
                case SpatialAwarenessSurfaceTypes.Unknown:
                    return new Color32(220, 50, 47, 255); // red
                case SpatialAwarenessSurfaceTypes.Floor:
                    return new Color32(38, 139, 210, 255); // blue
                case SpatialAwarenessSurfaceTypes.Ceiling:
                    return new Color32(108, 113, 196, 255); // violet
                case SpatialAwarenessSurfaceTypes.Wall:
                    return new Color32(181, 137, 0, 255); // yellow
                case SpatialAwarenessSurfaceTypes.Platform:
                    return new Color32(133, 153, 0, 255); // green
                case SpatialAwarenessSurfaceTypes.Background:
                    return new Color32(203, 75, 22, 255); // orange
                case SpatialAwarenessSurfaceTypes.World:
                    return new Color32(211, 54, 130, 255); // magenta
                case SpatialAwarenessSurfaceTypes.Inferred:
                    return new Color32(42, 161, 152, 255); // cyan
                default:
                    return new Color32(220, 50, 47, 255); // red
            }
        }

        private void ClearAndUpdateObserver()
        {
            ClearScene();
            observer.UpdateOnDemand();
        }

        private void ToggleObservedSurfaceType(SpatialAwarenessSurfaceTypes surfaceType)
        {
            if (observer.SurfaceTypes.HasFlag(surfaceType))
            {
                // Disable the original wall enable/disable function to avoid conflict
                if (surfaceType == SpatialAwarenessSurfaceTypes.Wall)
                {
                    print("Wall");
                }
                else
                {
                    observer.SurfaceTypes &= ~surfaceType;
                }
            }
            else
            {
                observer.SurfaceTypes |= surfaceType;
            }
        }

#endregion Helper Functions
    }
}
