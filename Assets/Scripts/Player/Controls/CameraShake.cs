using UnityEngine;
using System.Collections;

/// <summary>
/// Simple component for adding client side camera shake
/// </summary>
public class CameraShake : MonoBehaviour
{
    // Transform of the camera to shake. Grabs the gameObject's transform
    // if null.
    public LookAround camScript;

    // How long the object should shake for.
    public float shakeDuration = 0f;

    // Amplitude of the shake. A larger value shakes the camera harder.
    public float shakeAmount = 0.7f;
    public float decreaseFactor = 1.0f;

    private float currShakeDuration = 0;

    Vector3 originalPos;

    void Awake()
    {
        if (camScript == null)
        {
            camScript = GetComponent(typeof(LookAround)) as LookAround;
        }
    }

    void OnEnable()
    {
        currShakeDuration = shakeDuration;
    }

    void Update()
    {
        if (currShakeDuration > 0)
        {
            camScript.SetShakeOffset(Random.insideUnitSphere * shakeAmount);

            currShakeDuration -= Time.deltaTime * decreaseFactor;
        }
        else
        {
            currShakeDuration = 0f;
            camScript.SetShakeOffset(Vector3.zero);
            enabled = false;
        }
    }
}