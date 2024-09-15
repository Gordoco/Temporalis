using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkCameraToggle : MonoBehaviour
{
    private void Awake()
    {
        if (transform.root.name != "LocalGamePlayer") gameObject.SetActive(false);
    }
}
