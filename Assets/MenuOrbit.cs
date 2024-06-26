using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuOrbit : MonoBehaviour
{
    [SerializeField] private Transform OrbitPoint;

    [SerializeField] private float rotationSpeed = 10f;

    private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, float angle) 
    {
        Vector3 dir = point - pivot; // get point direction relative to pivot
        dir = Quaternion.Euler(new Vector3(0, angle, 0)) * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }

    // Update is called once per frame
    void Update()
    {
        Quaternion newRotation = Quaternion.LookRotation(OrbitPoint.position - transform.position);

        transform.position = RotatePointAroundPivot(transform.position, OrbitPoint.transform.position, rotationSpeed * Time.deltaTime);
        transform.rotation = newRotation;
    }
}
