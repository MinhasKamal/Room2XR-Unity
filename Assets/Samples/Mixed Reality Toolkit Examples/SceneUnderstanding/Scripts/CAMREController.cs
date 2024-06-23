using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

public class CAMREController : MonoBehaviour
{
    [SerializeField]
    private GameObject ServerController;
    [SerializeField]
    private Interactable PythonConnectToggle = null;
    // Start is called before the first frame update
    void Start()
    {
        //if (ServerController.GetComponent<TCPTestServer>() == true)
        //    PythonConnectToggle.IsToggled = true;
        //else
        //    PythonConnectToggle.IsToggled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleEnableTCPConnection()
    {
        if (PythonConnectToggle.IsToggled == true)
            ServerController.GetComponent<TCPTestServer>().enabled = true;
        else
            ServerController.GetComponent<TCPTestServer>().enabled = false;
    }
}
