using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoScreenshotter : MonoBehaviour
{
    private int rand = 0;
    private int count = 0;

    // Start is called before the first frame update
    void Start()
    {
        rand = Random.Range(0, 999999999);
        //StartCoroutine(AutoScreenshot());
    }

    private IEnumerator AutoScreenshot()
    {
        while (true)
        {
            yield return new WaitForSeconds(25);
            ScreenCapture.CaptureScreenshot(Application.dataPath + "/Screenshots/NEWSHOT" + count + rand + ".png");
            count++;
        }
    }
}
