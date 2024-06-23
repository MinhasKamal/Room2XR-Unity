using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDestroy : MonoBehaviour
{
    public GameObject destroiedObject;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DestroyObject()
    {
        Destroy(destroiedObject);
    }

}
