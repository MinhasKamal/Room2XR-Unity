using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Netcode.Transports.UTP;

public class RelayServerConnector : MonoBehaviour
{
    
    public async void JoinRelay(string joinCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(joinCode) == true)
            {

                joinCode = KeyBoardDisplayer.hostAddressKeyboard.text;
            }
            Debug.Log("Joining Relay with " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();

        } catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

}
