using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioListener))]
public class NetworkAudioToggle : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (transform.parent.name != "LocalGamePlayer")
        {
            GetComponent<AudioListener>().enabled = false;
        }
        transform.parent.gameObject.GetComponent<AudioListener>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
