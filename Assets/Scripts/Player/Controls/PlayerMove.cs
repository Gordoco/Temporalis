using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerMove : NetworkBehaviour
{
    //public float speed = 6.0f;
    //public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    private Vector3 moveDirection = Vector3.zero;
    public Vector3 StartLocation;
    [SyncVar] private bool bDead = false;

    private void Start()
    {
        if (!isOwned || bDead) enabled = false;
        transform.position = StartLocation;
    }

    [ClientRpc]
    public void SetDead()
    {
        bDead = true;
    }

    void Update() {
        if (!isOwned || bDead) return;

        CharacterController controller = GetComponent<CharacterController>();
        StatManager manager = GetComponent<StatManager>();
        if (controller.isGrounded)
        {
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection.Normalize();
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= (float)manager.GetStat(NumericalStats.MovementSpeed);
            if (Input.GetButton("Jump"))
                moveDirection.y = (float)manager.GetStat(NumericalStats.JumpHeight);
        }
        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
    }
}