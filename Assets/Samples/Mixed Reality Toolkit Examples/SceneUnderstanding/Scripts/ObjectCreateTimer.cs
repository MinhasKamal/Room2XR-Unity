using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Net;
using System.Net.Sockets;
using System.Globalization;

public class ObjectCreateTimer : MonoBehaviour
{
    private Vector3 previousObjectPosition;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(previousObjectPosition, this.transform.position) > 0.3f)
        {
            Debug.Log("Object: " + this.name + " has moved...");
            FileNameInitialize.writer.WriteLine("Object: " + this.name + " has moved at time: " + DateTime.Now.ToString("h:mm:ss.ffffff tt") + ", at position: " + this.transform.position + ", with rotation: " + this.transform.rotation);
            previousObjectPosition = this.transform.position;
        }
    }

    void Awake()
    {
        //if (TimeSyncInitial.tcpFlag == 0)
        //{
        //    string resp;
        //    using (var rdr = new StreamReader(TimeSyncInitial.tcp.GetStream()))
        //    {
        //        resp = rdr.ReadToEnd();
        //    }

        //    // string utc = resp.Substring(20, 17);
        //    //var dt = DateTimeOffset.ParseExact(utc,
        //    //           "yy - MM - dd HH: mm:ss",
        //    //   CultureInfo.InvariantCulture,
        //    //   DateTimeStyles.AssumeUniversal);
        //    print(resp);
        //    FileNameInitialize.writer.WriteLine(resp);
        //    // FileNameInitialize.writer.WriteLine('\n');
        //    // FileNameInitialize.writer.WriteLine('\n');
        //    TimeSyncInitial.tcpFlag = 1;
        //}

        FileNameInitialize.writer.WriteLine("Object with ID: " + this.name + " creation time: " + DateTime.Now.ToString("h:mm:ss.ffffff tt") + ", at position: " + this.transform.position + ", with rotation: " + this.transform.rotation);
        previousObjectPosition = this.transform.position;
        // print(DateTime.Now.ToString("h:mm:ss.ffffff tt"));
    }
}
