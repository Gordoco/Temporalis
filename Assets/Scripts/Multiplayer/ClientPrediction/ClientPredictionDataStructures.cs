
using System.Collections;
using UnityEngine;

public struct InputPayload
{
    public int tick;
    public Vector3 inputVector;
    public Quaternion inputRot;
}

public struct StatePayload
{
    public int tick;
    public Vector3 position;
    public Quaternion rotation;
}