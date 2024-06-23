// This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
// To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/ 
// or send a letter to Creative Commons, PO Box 1866, Mountain View, CA 94042, USA.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.MixedReality.Toolkit.WindowsSceneUnderstanding.Experimental;
using Photon.Pun;
using Photon.Realtime;

public class TCPTestServer : MonoBehaviour
{
	#region private members 	
	private TcpListener tcpListener;
	private Thread tcpListenerThread;
	private TcpClient connectedTcpClient;

	private byte[] transData;
	private byte[] transTriData;
	private byte[] transAllData;

	private String objectsString;

	private TimeSpan time_taken;
	#endregion

	#region Serializeable Fields
	// IP of server - Set in Unity Editor
	// [Tooltip("The IP of the Server.")]
	// [SerializeField] private string connectionIP = "192.168.50.7";

	// IP of server - Set in Unity Editor
	[Tooltip("Photon GameObject for transferring.")]
	[SerializeField] private GameObject NetworkTransferObject;

	// CAD Object Path - Set in Unity Editor
	[Tooltip("CAD Object Path, using folder name that contains CAD object files after MRTK.Tutorials.MultiUserCapabilities/Resources/")]
	[SerializeField] private string databasePath;
	#endregion

	// public string databasePath;

	private GameObject CADObjectScene;

	// Use this for initialization
	void Start()
	{
		CADObjectScene = GameObject.FindGameObjectWithTag("Scene");

		// Start TcpServer background thread 		
		tcpListenerThread = new Thread(new ThreadStart(ListenForIncommingRequests));
		tcpListenerThread.IsBackground = true;
		tcpListenerThread.Start();
	}

	// Update is called once per frame
	void Update()
	{
        // if (Input.GetKeyDown(KeyCode.Space))
        // if (Gamepad.current.rightTrigger.wasPressedThisFrame)
        // {
		//	Debug.Log("Object position saved...");
		//	FileNameInitialize.writer.WriteLine(" ");
		//	FileNameInitialize.writer.WriteLine("--------- Save All Object Position -----------------");
		//	for (int i = 0; i < CADObjectScene.transform.childCount; i++)
		//	{
		//		FileNameInitialize.writer.WriteLine("Object with ID: " + CADObjectScene.transform.GetChild(i).name + ", current position: " + CADObjectScene.transform.GetChild(i).position);
		//	}
		//	FileNameInitialize.writer.WriteLine("----------------------------------------------------");
		//	FileNameInitialize.writer.WriteLine(" ");
		//}
    }

	/// <summary> 	
	/// Runs in background TcpServerThread; Handles incomming TcpClient requests 	
	/// </summary> 	
	private void ListenForIncommingRequests()
	{
		try
		{
			// Create listener on localhost port 8052. 			
			tcpListener = new TcpListener(8052);
			tcpListener.Start();
			Debug.Log("Server is listening");
			connectedTcpClient = tcpListener.AcceptTcpClient();
		}
		catch (SocketException socketException)
		{
			Debug.Log("SocketException " + socketException.ToString());
		}
	}
	/// <summary> 	
	/// Send message to client using socket connection. 	
	/// </summary> 	
	private void SendMessage()
	{
		if (connectedTcpClient == null)
		{
			return;
		}

		try
		{
			VecToByteConverter();
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}

	public void VecToByteConverter()
    {
		NetworkStream stream = connectedTcpClient.GetStream();
		if (stream.CanWrite)
        {
            print(DateTime.Now.ToString("h:mm:ss.ffffff tt"));


            transData = new byte[sizeof(float) * 3 * Microsoft.MixedReality.Toolkit.Experimental.SceneUnderstanding.DemoSceneUnderstandingController.unityVertices.Length];

			Span<Vector3> TransDataSpan = new Span<Vector3>(Microsoft.MixedReality.Toolkit.Experimental.SceneUnderstanding.DemoSceneUnderstandingController.unityVertices);
			transData = MemoryMarshal.Cast<Vector3, byte>(TransDataSpan).ToArray();


            transTriData = new byte[sizeof(Int32) * Microsoft.MixedReality.Toolkit.Experimental.SceneUnderstanding.DemoSceneUnderstandingController.unityMeshTriangles.Length];

			Span<Int32> TransTriSpan = new Span<Int32>(Microsoft.MixedReality.Toolkit.Experimental.SceneUnderstanding.DemoSceneUnderstandingController.unityMeshTriangles);
			transTriData = MemoryMarshal.Cast<Int32, byte>(TransTriSpan).ToArray();

			// print(WindowsSceneUnderstandingObserver.positionVector);
			// print(WindowsSceneUnderstandingObserver.rotationVector);

			transAllData = new byte[sizeof(int) + sizeof(int) + sizeof(float) * 3 + sizeof(float) * 4 + transData.Length + transTriData.Length];
			BitConverter.GetBytes(Microsoft.MixedReality.Toolkit.Experimental.SceneUnderstanding.DemoSceneUnderstandingController.unityVertices.Length).CopyTo(transAllData, 0);
			BitConverter.GetBytes(Microsoft.MixedReality.Toolkit.Experimental.SceneUnderstanding.DemoSceneUnderstandingController.unityMeshTriangles.Length).CopyTo(transAllData, sizeof(int));

			BitConverter.GetBytes(WindowsSceneUnderstandingObserver.positionVector.x).CopyTo(transAllData, sizeof(int) + sizeof(int));
			BitConverter.GetBytes(WindowsSceneUnderstandingObserver.positionVector.y).CopyTo(transAllData, sizeof(int) + sizeof(int) + sizeof(float));
			BitConverter.GetBytes(WindowsSceneUnderstandingObserver.positionVector.z).CopyTo(transAllData, sizeof(int) + sizeof(int) + sizeof(float) * 2);

			BitConverter.GetBytes(WindowsSceneUnderstandingObserver.rotationVector.w).CopyTo(transAllData, sizeof(int) + sizeof(int) + sizeof(float) * 3);
			BitConverter.GetBytes(WindowsSceneUnderstandingObserver.rotationVector.x).CopyTo(transAllData, sizeof(int) + sizeof(int) + sizeof(float) * 3 + sizeof(float));
			BitConverter.GetBytes(WindowsSceneUnderstandingObserver.rotationVector.y).CopyTo(transAllData, sizeof(int) + sizeof(int) + sizeof(float) * 3 + sizeof(float) * 2);
			BitConverter.GetBytes(WindowsSceneUnderstandingObserver.rotationVector.z).CopyTo(transAllData, sizeof(int) + sizeof(int) + sizeof(float) * 3 + sizeof(float) * 3);

			transData.CopyTo(transAllData, sizeof(int) + sizeof(int) + sizeof(float) * 3 + sizeof(float) * 4);
			transTriData.CopyTo(transAllData, sizeof(int) + sizeof(int) + sizeof(float) * 3 + sizeof(float) * 4 + transData.Length);
			stream.Write(transAllData, 0, transAllData.Length);

			// print(DateTime.Now.ToString("h:mm:ss.ffffff tt"));
			Debug.Log("Server sent his message - should be received by client");
		}

		ServerJsonReceiver(stream);
	}

	// Receive and process json data from CAD python side
	private void ServerJsonReceiver(NetworkStream stream)
    {
		Byte[] receiveBytes = new Byte[sizeof(float) * 1024];
		Byte[] dictionaryByte = new Byte[sizeof(Int32)];
		Int32 dictionaryLength = 0;
		Int32 curDictLength = 0;
		int length;
		// Read length of the dictionary
		if ((length = stream.Read(dictionaryByte, 0, dictionaryByte.Length)) != 0)
		{
			dictionaryLength = BitConverter.ToInt32(dictionaryByte, 0);
			print(dictionaryLength);
		}
		Byte[] TransDictionary = new Byte[dictionaryLength];
		// Read incomming stream into byte arrary. 					
		while ((length = stream.Read(receiveBytes, 0, receiveBytes.Length)) != 0)
		{
			var incommingData = new byte[length];

			Array.Copy(receiveBytes, 0, TransDictionary, curDictLength, length);
			curDictLength = curDictLength + length;

			if (curDictLength == TransDictionary.Length)
				break;
		}
		objectsString = System.Text.Encoding.UTF8.GetString(TransDictionary, 0, dictionaryLength);
		print("Json String Received with Length: " + objectsString.Length.ToString());

		var t_start = System.DateTime.Now;
		// Set up data path for CAD prefab files (Prefab folders should be put in "Assets/MRTK.Tutorials.MultiUserCapabilities/Resources/"), OldPrefabFiles/ is used for testing, CADPrefabObjects/ is used for HL2 usage)
		vrrec.JsonSceneProcessor(objectsString, databasePath);

		time_taken = System.DateTime.Now - t_start;

		FileNameInitialize.writer.WriteLine("----------------------------------------------------------");
		FileNameInitialize.writer.WriteLine("Total CAD Objects Loading Time: " + time_taken.ToString());
		FileNameInitialize.writer.WriteLine("----------------------------------------------------------");
		FileNameInitialize.writer.WriteLine(" ");
	}
}