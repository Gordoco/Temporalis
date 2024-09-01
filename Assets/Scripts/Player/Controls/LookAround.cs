using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

[Serializable]
public struct View 
{
    public Vector3 loc;
    public Vector3 rot;

    public View(Vector3 inLoc, Vector3 inRot) { loc = inLoc; rot = inRot; }
}

/// <summary>
/// Handler for Player camera control
/// </summary>
public class LookAround : NetworkBehaviour
{
    public float mouseXSensitivity = 100f;
    public float mouseYSensitivity = 1f;
    public Transform playerBody;
    [SerializeField] GameObject Weapon;
    [SerializeField] GameObject UI;
    public bool bIsServer = false;

    [SerializeField] private View TopDownView = new View(new Vector3(0, 3, -2), new Vector3(52, 0, 0));
    [SerializeField] private View StraightView = new View(new Vector3(0, 2.23f, -4.18f), new Vector3(10, 0, 0));
    [SerializeField] private View DownTopView = new View(new Vector3(0, -0.27f, -1), new Vector3(-70, 0, 0));
    private float yRotation = 0.3f;
    private Vector3 weaponMiddle;
    private Vector3 shakeOffset = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        if (!isOwned)
        {
            gameObject.GetComponent<Camera>().enabled = false;
            UI.SetActive(false);
            return;
        }

        if (Weapon != null) weaponMiddle = Weapon.transform.localPosition;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!isOwned) return;

        UpdateFunctionality();
    }

    public void SetShakeOffset(Vector3 inShake)
    {
        shakeOffset = inShake;
    }

    void UpdateFunctionality()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseXSensitivity * Time.smoothDeltaTime;
        float mouseY = -1 * Input.GetAxis("Mouse Y") * mouseYSensitivity * Time.smoothDeltaTime;

        yRotation = Mathf.Clamp(yRotation + mouseY, 0, 1);

        UpdateWeapon(yRotation);

        Vector3 pos;
        if (yRotation <= 0.5)
            pos = new Vector3(
                Mathf.Lerp(DownTopView.loc.x, StraightView.loc.x, yRotation * 2),
                Mathf.Lerp(DownTopView.loc.y, StraightView.loc.y, yRotation * 2),
                Mathf.Lerp(DownTopView.loc.z, StraightView.loc.z, yRotation * 2)
            );
        else
            pos = new Vector3(
                Mathf.Lerp(StraightView.loc.x, TopDownView.loc.x, (yRotation - 0.5f) * 2),
                Mathf.Lerp(StraightView.loc.y, TopDownView.loc.y, (yRotation - 0.5f) * 2),
                Mathf.Lerp(StraightView.loc.z, TopDownView.loc.z, (yRotation - 0.5f) * 2)
            );

        Quaternion rot;
        if (yRotation <= 0.5)
            rot = Quaternion.Euler(new Vector3(
                Mathf.Lerp(DownTopView.rot.x, StraightView.rot.x, yRotation * 2),
                Mathf.Lerp(DownTopView.rot.y, StraightView.rot.y, yRotation * 2),
                Mathf.Lerp(DownTopView.rot.z, StraightView.rot.z, yRotation * 2)
            ));
        else
            rot = Quaternion.Euler(new Vector3(
                Mathf.Lerp(StraightView.rot.x, TopDownView.rot.x, (yRotation - 0.5f) * 2),
                Mathf.Lerp(StraightView.rot.y, TopDownView.rot.y, (yRotation - 0.5f) * 2),
                Mathf.Lerp(StraightView.rot.z, TopDownView.rot.z, (yRotation - 0.5f) * 2)
            ));

        transform.localPosition = pos + shakeOffset;
        transform.localRotation = rot;

        //X Rotation
        playerBody.transform.rotation = Quaternion.Euler(playerBody.transform.rotation.eulerAngles + Vector3.up * mouseX);
    }

    void UpdateWeapon(float rot)
    {
        if (Weapon == null) return;
        Weapon.transform.localPosition = new Vector3(Weapon.transform.localPosition.x, Mathf.Lerp(weaponMiddle.y + 1f, weaponMiddle.y - 1f, rot), Weapon.transform.localPosition.z);
        Weapon.transform.localRotation = Quaternion.Euler(new Vector3(Mathf.Lerp(-45, 45, rot), 1, 1));
    }
}
