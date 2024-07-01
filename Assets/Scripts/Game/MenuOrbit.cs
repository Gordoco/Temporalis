using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuOrbit : MonoBehaviour
{
    [SerializeField] private Transform OrbitPoint;

    [SerializeField] private float rotationSpeed = 10f;

    [SerializeField] private Transform HostPanLocation;

    [SerializeField] private float PositionChangeSpeed = 1f;

    bool bPanning = true;
    bool bHostHovered = false;
    bool bHostStopHovered = false;

    private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, float angle) 
    {
        Vector3 dir = point - pivot; // get point direction relative to pivot
        dir = Quaternion.Euler(new Vector3(0, angle, 0)) * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }

    float hostPositionProg = 0;
    Vector3 PrevPanPoint = Vector3.zero;
    Quaternion PrevPanRotation = Quaternion.identity;

    private void Start()
    {
        PrevPanPoint = transform.position;
        PrevPanRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (bPanning)
        {
            Quaternion newRotation = Quaternion.LookRotation(OrbitPoint.position - transform.position);

            transform.position = RotatePointAroundPivot(transform.position, OrbitPoint.transform.position, rotationSpeed * Time.deltaTime);
            transform.rotation = newRotation;
        }
        else if (bHostHovered)
        {
            transform.position = Vector3.Lerp(PrevPanPoint, HostPanLocation.position, hostPositionProg);
            transform.rotation = Quaternion.Lerp(PrevPanRotation, HostPanLocation.rotation, hostPositionProg);
            if (hostPositionProg != 1) hostPositionProg += Time.deltaTime * PositionChangeSpeed;
            if (hostPositionProg > 1) hostPositionProg = 1;
        }
        else if (bHostStopHovered)
        {
            transform.position = Vector3.Lerp(PrevPanPoint, HostPanLocation.position, hostPositionProg);
            transform.rotation = Quaternion.Lerp(PrevPanRotation, HostPanLocation.rotation, hostPositionProg);
            if (hostPositionProg != 0) hostPositionProg -= Time.deltaTime * PositionChangeSpeed;
            if (hostPositionProg <= 0)
            {
                hostPositionProg = 0;
                bHostStopHovered = false;
                bPanning = true;
            }
        }
    }

    public void OnHostStartHover()
    {
        if (bPanning)
        {
            PrevPanPoint = transform.position;
            PrevPanRotation = transform.rotation;
        }
        bPanning = false;
        bHostStopHovered = false;
        bHostHovered = true;
    }
    public void OnHostStopHover()
    {
        bPanning = false;
        bHostHovered = false;
        bHostStopHovered = true;
    }
}
