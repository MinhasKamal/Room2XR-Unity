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

public class TCPTestClient : MonoBehaviour
{
	#region private members 	
	private TcpClient socketConnection;
	private Thread clientReceiveThread;
	// private byte[] receiveBytes;
	#endregion

	#region Serializeable Fields
	// IP of server - Set in Unity Editor
	[Tooltip("The IP of the Server to connect to.")]
	[SerializeField] private string connectionIP = "192.168.50.7";
	#endregion

	#region Public Fields
	public static Vector3[] InVerVec;
	public static Int32[] InTriVec;
	public static Vector3 positionVector = Vector3.zero;
	public static Quaternion rotationVector = Quaternion.identity;
	#endregion

	// Use this for initialization 	
	void Start()
	{
		ConnectToTcpServer();
	}
	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			print("Message Sent!!!");
			SendMessage();
		}
	}
	/// <summary> 	
	/// Setup socket connection. 	
	/// </summary> 	
	private void ConnectToTcpServer()
	{
		try
		{
			clientReceiveThread = new Thread(new ThreadStart(ListenForData));
			clientReceiveThread.IsBackground = true;
			clientReceiveThread.Start();
			// Debug.Log("Connected Successfully!!!");
		}
		catch (Exception e)
		{
			Debug.Log("On client connect exception " + e);
		}
	}
	/// <summary> 	
	/// Runs in background clientReceiveThread; Listens for incomming data. 	
	/// </summary>     
	private void ListenForData()
	{
		try
		{
			socketConnection = new TcpClient(connectionIP, 8052);
			Byte[] VerticesByte = new Byte[sizeof(Int32)];
			Byte[] TriangleByte = new Byte[sizeof(Int32)];
			Byte[] PositionByte = new Byte[sizeof(float) * 3];
			Byte[] RotationByte = new Byte[sizeof(float) * 4];
			Byte[] receiveBytes = new Byte[sizeof(float) * 1024];
			Vector3 inVect = Vector3.zero;
			Int32[] TriangleSet = new Int32[3];
			Int32 verLength = 0;
			Int32 triLength = 0;

			Int32 curAllLength = 0;

			Int32 headerFlag = 0;

			List<Vector3> MeshVertices = new List<Vector3>();
			List<Int32> MeshTriangle = new List<Int32>();

			// Vector3[] InVerVec;
			// Int32[] InTriVec;

			while (true)
			{
				NetworkStream stream = socketConnection.GetStream();

				Array.Clear(VerticesByte, 0, VerticesByte.Length);
				Array.Clear(TriangleByte, 0, TriangleByte.Length);
				Array.Clear(PositionByte, 0, PositionByte.Length);
				Array.Clear(RotationByte, 0, RotationByte.Length);
				curAllLength = 0;

				print("Got something!");
				int length;
				// Read incomming header for triangle list length
				if ((length = stream.Read(VerticesByte, 0, VerticesByte.Length)) != 0)
				{
					verLength = BitConverter.ToInt32(VerticesByte, 0);
					InVerVec = new Vector3[verLength];
					// print(verLength);
				}

				// Read incomming header for triangle list length
				if ((length = stream.Read(TriangleByte, 0, TriangleByte.Length)) != 0)
				{
					triLength = BitConverter.ToInt32(TriangleByte, 0);
					InTriVec = new Int32[triLength];
					// print(triLength);
				}

				// Read incomming position vector
				if ((length = stream.Read(PositionByte, 0, PositionByte.Length)) != 0)
				{
					positionVector.x = BitConverter.ToSingle(PositionByte, 0);
					positionVector.y = BitConverter.ToSingle(PositionByte, sizeof(float));
					positionVector.z = BitConverter.ToSingle(PositionByte, sizeof(float) * 2);
				}

				// Read incomming rotation vector
				if ((length = stream.Read(RotationByte, 0, RotationByte.Length)) != 0)
				{
					rotationVector.w = BitConverter.ToSingle(RotationByte, 0);
					rotationVector.x = BitConverter.ToSingle(RotationByte, sizeof(float));
					rotationVector.y = BitConverter.ToSingle(RotationByte, sizeof(float) * 2);
					rotationVector.z = BitConverter.ToSingle(RotationByte, sizeof(float) * 3);
				}

				print(positionVector);
				print(rotationVector);

				Byte[] TransAllData = new Byte[sizeof(float) * 3 * verLength + sizeof(Int32) * triLength];

				// Read incomming stream into byte arrary. 					
				while ((length = stream.Read(receiveBytes, 0, receiveBytes.Length)) != 0)
				{
					var incommingData = new byte[length];

					// Array.Copy(receiveBytes, 0, incommingData, 0, length);
					Array.Copy(receiveBytes, 0, TransAllData, curAllLength, length);
					curAllLength = curAllLength + length;

					if (curAllLength == TransAllData.Length)
						break;
				}

				Span<Byte> TransDataSpan = new Span<Byte>(TransAllData);
				// print(TransDataSpan.Length);
				Span<Byte> VerticesSpan = TransDataSpan.Slice(0, sizeof(float) * 3 * verLength);
				// print(VerticesSpan.Length);
				InVerVec = MemoryMarshal.Cast<Byte, Vector3>(VerticesSpan).ToArray();
				// print(InVerVec[0]);
				print(InVerVec.Length);

				Span<Byte> TriangleSpan = TransDataSpan.Slice(sizeof(float) * 3 * verLength, sizeof(Int32) * triLength);
				InTriVec = MemoryMarshal.Cast<Byte, Int32>(TriangleSpan).ToArray();
				// print(InTriVec[0]);
				print(InTriVec.Length);
				// print(TriangleSpan.Length);
				print(DateTime.Now.ToString("h:mm:ss.ffffff tt"));

				MeshInitializer.meshTestFlag = 0;
				// Get a stream object for reading 				
				

				//if (MeshVertices.Count == verLength && MeshTriangle.Count == triLength)
				//	break;
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}
	/// <summary> 	
	/// Send message to server using socket connection. 	
	/// </summary> 	
	private void SendMessage()
	{
		if (socketConnection == null)
		{
			print("No Connection...");
			return;
		}
		try
		{
			// Get a stream object for writing. 			
			NetworkStream stream = socketConnection.GetStream();
			if (stream.CanWrite)
			{
				string clientMessage = "This is a message from one of your clients.";
				// Convert string message to byte array.                 
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
				// Write byte array to socketConnection stream.                 
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
				Debug.Log("Client sent his message - should be received by server");
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}
}