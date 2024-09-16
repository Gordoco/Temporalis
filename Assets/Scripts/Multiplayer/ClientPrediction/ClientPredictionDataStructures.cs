
using System.Collections;
using UnityEngine;

public struct InputPayload
{
    public int tick;
    public Vector3 inputVector;
    public Vector3 inputScale;
    public Quaternion inputRot;
}

public struct StatePayload
{
    public int tick;
    public Vector3 position;
    public Vector3 velocity;
    public Vector3 scale;
    public Quaternion rotation;
}