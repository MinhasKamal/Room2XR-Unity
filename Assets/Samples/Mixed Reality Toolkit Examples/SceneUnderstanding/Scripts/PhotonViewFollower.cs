using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonViewFollower : MonoBehaviour
{
    private GameObject RemoteUserView;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameObject.Find("User1") != null)
        {
            RemoteUserView = GameObject.Find("User1");
            Camera.main.transform.position = new Vector3(RemoteUserView.transform.position.x, RemoteUserView.transform.position.y, RemoteUserView.transform.position.z);
            Camera.main.transform.eulerAngles = new Vector3(RemoteUserView.transform.eulerAngles.x, RemoteUserView.transform.eulerAngles.y, RemoteUserView.transform.eulerAngles.z);
        }
    }
}
