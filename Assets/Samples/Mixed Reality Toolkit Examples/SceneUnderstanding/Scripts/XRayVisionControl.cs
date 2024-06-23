using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;


public class XRayVisionControl : MonoBehaviour
{
    [SerializeField]
    private TextMeshPro XRaySizeTextMesh = null;
    [SerializeField]
    private GameObject XRayVisionWindow = null;
    [SerializeField]
    private Interactable CubeCreater = null;

    // Start is called before the first frame update
    void Start()
    {
        XRaySizeTextMesh.text = $"{0.3:F2}";
        CubeCreater.IsToggled = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void XRaySizeChangeOnSliderUpdated(SliderEventData eventData)
    {
        if (eventData.NewValue == 0)
        {
            XRayVisionWindow.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            XRaySizeTextMesh.text = $"{0.01f:F2}";
        }
        else
        {
            XRayVisionWindow.transform.localScale = new Vector3((float)eventData.NewValue, (float)eventData.NewValue, (float)eventData.NewValue);
            XRaySizeTextMesh.text = $"{(float)eventData.NewValue:F2}";
        }
    }

    public void PressableCreateCube(GameObject cubePrefab)
    {
        Vector3 zPosition = new Vector3(0, 0, 0.3f);
        // Instantiate(cubePrefab, Camera.main.transform.position + zPosition, Quaternion.identity);
        var boxPrefab = PhotonNetwork.Instantiate("OldPrefabFiles/02933112/36bfa6f0a5897be786d53ab0fe94e911/" + cubePrefab.name, Camera.main.transform.position + zPosition, Quaternion.identity);
    }

    public void TouchDestroy(GameObject cubePrefab)
    {
        cubePrefab = GameObject.Find("fe2c951f711bf1971a12d98ab62a7b8e(Clone)");
        PhotonNetwork.Destroy(cubePrefab.GetComponent<PhotonView>());
    }
}