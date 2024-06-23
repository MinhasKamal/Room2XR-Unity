using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine.UI;
using TMPro;

public class MinimapControl : MonoBehaviour
{
    [SerializeField]
    private Interactable minimapOption = null;
    [SerializeField]
    private RawImage minimapImage = null;
    [SerializeField]
    private Image Border = null;
    [SerializeField]
    private TextMeshPro camPositionTextMesh = null;
    [SerializeField]
    private TextMeshPro FOVtextMesh = null;

    private GameObject minimapCamera;
    private Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        minimapOption.IsToggled = true;

        minimapCamera = GameObject.Find("Minimap Camera");
        cam = minimapCamera.GetComponent<Camera>();

        camPositionTextMesh.text = $"{25:F2}";
        FOVtextMesh.text = $"{20:F2}";
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ToggleMinimapSwitch()
    {
        if (minimapOption.IsToggled == true)
        {
            minimapImage.enabled = true;
            Border.enabled = true;
        }
        else
        {
            minimapImage.enabled = false;
            Border.enabled = false;
        }
    }

    public void CameraPositionOnSliderUpdated(SliderEventData eventData)
    {
        ObjectFollowPosition.cameraYDistance = 60 * (float)eventData.NewValue + 2;
        camPositionTextMesh.text = $"{(60 * (float)eventData.NewValue + 2):F2}";
    }

    public void FOVOnSliderUpdated(SliderEventData eventData)
    {
        cam.fieldOfView = 60 * (float)eventData.NewValue + 1;
        FOVtextMesh.text = $"{(60 * (float)eventData.NewValue + 1):F2}";
    }
}
