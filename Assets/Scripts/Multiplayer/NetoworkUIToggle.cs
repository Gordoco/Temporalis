using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetoworkUIToggle : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (transform.root.name != "LocalGamePlayer") gameObject.SetActive(false);
    }
}
