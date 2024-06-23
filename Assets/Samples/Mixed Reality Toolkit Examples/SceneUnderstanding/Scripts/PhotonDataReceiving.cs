using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using MRTK.Tutorials.MultiUserCapabilities;

public class PhotonDataReceiving : MonoBehaviour, IPunInstantiateMagicCallback
{
    string objectsString;
    byte[] byteData = new byte[6540];
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        print("Byte data received");
        object[] instantiationData = info.photonView.InstantiationData;
        // print(instantiationData.Length);
        // print(instantiationData[0]);
        instantiationData.CopyTo(byteData, 0);
        objectsString = System.Text.Encoding.UTF8.GetString(byteData, 0, byteData.Length);
        vrrec.JsonSceneProcessor(objectsString, Application.streamingAssetsPath + "/ShapeNetV2_Stream");
        // print(objectsString);
    }

}
