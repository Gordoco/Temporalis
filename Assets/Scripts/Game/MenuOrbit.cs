using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuOrbit : MonoBehaviour
{
    [SerializeField] private Transform OrbitPoint;

    [SerializeField] private float rotationSpeed = 10f;

    [SerializeField] private Transform HostPanLocation;
    [SerializeField] private Transform SettingsPanLocation;

    [SerializeField] private float PositionChangeSpeed = 1f;

    bool bPanning = true;
    bool bFixed = false;

    /*bool bHostHovered = false;
    bool bHostStopHovered = false;*/

    private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, float angle) 
    {
        Vector3 dir = point - pivot; // get point direction relative to pivot
        dir = Quaternion.Euler(new Vector3(0, angle, 0)) * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }

    bool bReset = true;
    //float hostPositionProg = 0;
    Vector3 PrevPanPoint = Vector3.zero;
    Quaternion PrevPanRotation = Quaternion.identity;

    Vector3 PrevPoint = Vector3.zero;
    Quaternion PrevRotation = Quaternion.identity;

    private void Start()
    {
        PrevPanPoint = transform.position;
        PrevPanRotation = transform.rotation;
    }

    float transitionProgress = 0;
    Transform fixedPoint;

    // Update is called once per frame
    void Update()
    {
        if (bPanning)
        {
            if (!bReset && (transform.position != PrevPoint || transform.rotation != PrevRotation))
            {
                ResetToPan();
            }
            else
            {
                //Continue Panning
                PanLogic();
            }
        }
        else if (bFixed)
        {
            bReset = false;
            transform.position = Vector3.Lerp(PrevPoint, fixedPoint.position, transitionProgress);
            transform.rotation = Quaternion.Lerp(PrevRotation, fixedPoint.rotation, transitionProgress);
            if (transitionProgress != 1) transitionProgress += Time.smoothDeltaTime * PositionChangeSpeed;
            if (transitionProgress > 1) transitionProgress = 1;
        }
    }

    private void PanLogic()
    {
        Quaternion newRotation = Quaternion.LookRotation(OrbitPoint.position - transform.position);

        transform.position = RotatePointAroundPivot(transform.position, OrbitPoint.transform.position, rotationSpeed * Time.smoothDeltaTime);
        transform.rotation = newRotation;
    }

    private void ResetToPan()
    {
        transform.position = Vector3.Lerp(PrevPoint, fixedPoint.position, transitionProgress);
        transform.rotation = Quaternion.Lerp(PrevRotation, fixedPoint.rotation, transitionProgress);
        if (transitionProgress != 0) transitionProgress -= Time.smoothDeltaTime * (PositionChangeSpeed/2);
        if (transitionProgress < 0) transitionProgress = 0;
        if (transitionProgress == 0) bReset = true;
    }

    bool bCooldown = false;

    private void ChangeToFixCameraPos(Transform fixedPos)
    {
        if (bCooldown) return;
        bCooldown = true;
        StartCoroutine(Cooldown());

        if (bReset)
        {
            PrevPanPoint = transform.position;
            PrevPanRotation = transform.rotation;
        }
        PrevPoint = transform.position;
        PrevRotation = transform.rotation;
        //if (fixedPoint != fixedPos) transitionProgress = 0;
        bPanning = false;
        bFixed = true;
        fixedPoint = fixedPos;
    }

    private void ResetToCameraPanning()
    {
        bPanning = true;
        bFixed = false;
        PrevPoint = PrevPanPoint;
        PrevRotation = PrevPanRotation;
    }

    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(0.5f);
        bCooldown = false;
    }

    public void OnHostStartHover()
    {
        ChangeToFixCameraPos(HostPanLocation);
    }
    public void OnHostStopHover()
    {
        ResetToCameraPanning();
    }

    public void OnSettingsStartHover()
    {
        ChangeToFixCameraPos(SettingsPanLocation);
    }

    public void OnSettingsStopHover()
    {
        ResetToCameraPanning();
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
