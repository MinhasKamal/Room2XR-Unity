using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Collections;

public class PlayerIDSetting : NetworkBehaviour
{
    [SerializeField] private TMP_Text PlayerNameID;
    private NetworkVariable<FixedString128Bytes> networkPlayerName = new NetworkVariable<FixedString128Bytes>(
        "Player: 0", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        networkPlayerName.Value = "Player: " + (OwnerClientId + 1);
        PlayerNameID.text = networkPlayerName.Value.ToString();
    }

}
