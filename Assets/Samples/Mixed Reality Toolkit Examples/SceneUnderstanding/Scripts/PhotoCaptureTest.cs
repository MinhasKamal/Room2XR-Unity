using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using Microsoft.MixedReality.Toolkit.Experimental;
using Microsoft.MixedReality.Toolkit.Experimental.SceneUnderstanding;
#if ENABLE_WINMD_SUPPORT

#endif

public class PhotoCaptureTest : MonoBehaviour
{
#if ENABLE_WINMD_SUPPORT
    private UnityEngine.Windows.WebCam.PhotoCapture photoCaptureObject = null;
#endif
    private int photoFlag;
    public static float HSVVValue;
    private int listLength;
    private int lateStartFlag = 0;
    private GameObject clippingBox;
    // Start is called before the first frame update
    void Start()
    {
        clippingBox = GameObject.Find("ClippingBox");
        photoFlag = 0;
#if ENABLE_WINMD_SUPPORT
        UnityEngine.Windows.WebCam.PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
        print("OnPhotoCaptureCreated Done");
#endif
    }

    // Update is called once per frame
    void Update()
    {
        if (lateStartFlag == 0)
        {
            photoFlag = 0;
#if ENABLE_WINMD_SUPPORT
            print(photoCaptureObject);
#endif
            lateStartFlag = 1;
        }
        if (DemoSceneUnderstandingController.photoCaptureFlag == 1)
        {
            // print(photoFlag);
            if (photoFlag == 30)
            {
                // Stopwatch stopwatch = new Stopwatch();
                // stopwatch.Start();
#if ENABLE_WINMD_SUPPORT
                photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
#endif
                // stopwatch.Stop();
                // TimeSpan ts = stopwatch.Elapsed;
                // string responseTime = String.Format("{0:0.00}", ts.TotalMilliseconds);
                // print(responseTime);
                photoFlag = 0;
            }
            photoFlag += 1;
        }
    }
#if ENABLE_WINMD_SUPPORT

    void OnPhotoCaptureCreated(UnityEngine.Windows.WebCam.PhotoCapture captureObject)
    {
        photoCaptureObject = captureObject;

        Resolution cameraResolution = UnityEngine.Windows.WebCam.PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

        UnityEngine.Windows.WebCam.CameraParameters c = new UnityEngine.Windows.WebCam.CameraParameters();
        c.hologramOpacity = 0.0f;
        c.cameraResolutionWidth = cameraResolution.width;
        c.cameraResolutionHeight = cameraResolution.height;
        c.pixelFormat = UnityEngine.Windows.WebCam.CapturePixelFormat.BGRA32;

        captureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);
    }

    void OnStoppedPhotoMode(UnityEngine.Windows.WebCam.PhotoCapture.PhotoCaptureResult result)
    {
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }

    private void OnPhotoModeStarted(UnityEngine.Windows.WebCam.PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
        }
        else
        {
            // Debug.LogError("Unable to start photo mode!");
            print("Unable to start photo mode!");
        }
    }

    void OnCapturedPhotoToMemory(UnityEngine.Windows.WebCam.PhotoCapture.PhotoCaptureResult result, UnityEngine.Windows.WebCam.PhotoCaptureFrame photoCaptureFrame)
    {
        if (result.success)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<byte> imageBufferList = new List<byte>();
            // Copy the raw IMFMediaBuffer data into our empty byte list.
            photoCaptureFrame.CopyRawImageDataIntoBuffer(imageBufferList);

            // In this example, we captured the image using the BGRA32 format.
            // So our stride will be 4 since we have a byte for each rgba channel.
            // The raw image data will also be flipped so we access our pixel data
            // in the reverse order.
            int stride = 4;
            float denominator = 1.0f / 255.0f;
            HSVVValue = 0.0f;
            listLength = imageBufferList.Count;
            List<Color> colorArray = new List<Color>();
            for (int i = imageBufferList.Count - 1; i >= 0; i -= stride)
            {
                float a = (int)(imageBufferList[i - 0]) * denominator;
                float r = (int)(imageBufferList[i - 1]) * denominator;
                float g = (int)(imageBufferList[i - 2]) * denominator;
                float b = (int)(imageBufferList[i - 3]) * denominator;

                colorArray.Add(new Color(r, g, b, a));
                HSVVValue += ((Math.Max(Math.Max(r, g), b) + Math.Min(Math.Min(r, g), b)) / 2);
            }
            // Now we could do something with the array such as texture.SetPixels() or run image processing on the list
            // print(colorArray[0]);
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            string responseTime = String.Format("{0:0.00}", ts.TotalMilliseconds);
            print(responseTime);
            // print(HSVVValue / (imageBufferList.Count / 4));
        }
        print(HSVVValue / (listLength / 4) * 100);
        clippingBox.transform.localScale = new Vector3(0.3f / (HSVVValue / (listLength / 4) / 0.45f),
                                                        0.3f / (HSVVValue / (listLength / 4) / 0.45f),
                                                        0.3f / (HSVVValue / (listLength / 4) / 0.45f));
        // photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }
#endif
    void OnDestroy()
    {
#if ENABLE_WINMD_SUPPORT
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
#endif
    }
}
