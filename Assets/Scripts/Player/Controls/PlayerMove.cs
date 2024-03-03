using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerMove : NetworkBehaviour
{
    public float speed = 6.0f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    private Vector3 moveDirection = Vector3.zero;

    private int test;
    private void Start()
    {
        if (!isOwned) enabled = false;
        test = Random.Range(0, 100000);
        DontDestroyOnLoad(this.gameObject);
    }

    void Update() {
        Debug.Log("Shouldn't Just See Me: " + test);
        if (!isOwned) return;

        Debug.Log("Owned Client Trying to Move: " + test);
        CharacterController controller = GetComponent<CharacterController>();
        Debug.Log(controller.isGrounded + ": " + test);
        if (controller.isGrounded)
        {
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection.Normalize();
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= speed;
            if (Input.GetButton("Jump"))
                moveDirection.y = jumpSpeed;
        }
        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
        //CmdUpdateFunctionality(moveDirection);
    }

    [Command]
    void CmdUpdateFunctionality(Vector3 moveDirection)
    {
        Debug.Log("ServerShouldBeRunningThis: " + test);
        ServerUpdateFunc(moveDirection);
    }

    [Server]
    void ServerUpdateFunc(Vector3 moveDirection)
    {
        CharacterController controller = GetComponent<CharacterController>();
        controller.Move(moveDirection * Time.deltaTime);
        Debug.Log("Server Should Have Moved Us: " + test);
    }
}