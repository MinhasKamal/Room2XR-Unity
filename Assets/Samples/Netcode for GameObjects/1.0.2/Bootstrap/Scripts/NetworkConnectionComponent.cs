using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkConnectionComponent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ToggleHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void ToggleClient()
    {
        NetworkManager.Singleton.StartClient();
    }
}


