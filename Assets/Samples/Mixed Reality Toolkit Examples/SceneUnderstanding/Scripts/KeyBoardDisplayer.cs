using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KeyBoardDisplayer : MonoBehaviour
{
    public TMP_Text hostAddress;
    public static TouchScreenKeyboard hostAddressKeyboard;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (hostAddressKeyboard != null)
        {
            hostAddress.text = hostAddressKeyboard.text;
        }
        else
        {
            hostAddress.text = "<-- Enter\nhost address";
        }
    }

    public void OpenSystemKeyboard()
    {
        hostAddressKeyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, false, false);
    }
}
