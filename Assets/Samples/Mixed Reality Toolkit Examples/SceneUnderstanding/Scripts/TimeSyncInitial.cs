using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Globalization;

public class TimeSyncInitial : MonoBehaviour
{
    public static TcpClient tcp;
    public static int tcpFlag;

    // Start is called before the first frame update
    void Start()
    {
        tcp = new TcpClient("time-a-g.nist.gov", 13);
        tcpFlag = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
