using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkObjectSpawing : MonoBehaviour
{
    [SerializeField] private GameObject spawnedObjectPrefab;
    [SerializeField] private GameObject spawnedObjectParent;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleSpawnedObject()
    {
        GameObject spawnedGameObject = Instantiate(spawnedObjectPrefab);
        spawnedGameObject.GetComponent<NetworkObject>().Spawn(true);
    }

    public void ToggleRemoveObject()
    {
        for (var i = spawnedObjectParent.transform.childCount - 1; i>=0; i--)
        {
            Destroy(spawnedObjectParent.transform.GetChild(i).gameObject);
        }
    }
}
