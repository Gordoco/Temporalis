using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkCameraToggle : MonoBehaviour
{
    private void Start()
    {
        if (transform.root.name != "LocalGamePlayer") GetComponent<Camera>().enabled = false;
    }
}
