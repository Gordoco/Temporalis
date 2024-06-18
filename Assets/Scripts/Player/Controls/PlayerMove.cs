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
    private bool bFlying = false;
    [SyncVar] private bool bDead = false;
    private bool bAwake = false;
    private float tempGravity = 0;
    private int AnimMovingHash;
    private int AnimStrafingHash;
    private int AnimJumpingHash;
    Animator childAnimator;

    private void Awake()
    {
        AnimMovingHash = Animator.StringToHash("Running");
        AnimStrafingHash = Animator.StringToHash("Strafing");
        AnimJumpingHash = Animator.StringToHash("Jumping");
        childAnimator = GetComponentInChildren<Animator>();
    }

    public void SetFlying(bool b)
    {
        if (b)
        {
            moveDirection.y = 0;
            bFlying = true;
            tempGravity = -1 * (2 * gravity);
        }
        else if (bFlying)
        {
            bFlying = false;
            tempGravity = 0;
        }
    }

    public void SetTempGravity(float val) { tempGravity = val; bFlying = false; }

    private void Start()
    {
        if (!isOwned || bDead) enabled = false;
        StartCoroutine(Delay());
    }

    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(1);
        transform.position = new Vector3(Random.Range(-20f, 20f), 100, Random.Range(-20.0f, 20.0f));
        bAwake = true;
    }

    [ClientRpc]
    public void SetDead()
    {
        bDead = true;
    }

    void Update()
    {
        if (!isOwned || bDead || !bAwake) return;

        CharacterController controller = GetComponent<CharacterController>();
        if (!controller.enabled) return;
        StatManager manager = GetComponent<StatManager>();
        if (controller.isGrounded)
        {
            childAnimator.SetBool(AnimJumpingHash, false);
            tempGravity = 0;
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection.Normalize();
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= (float)manager.GetStat(NumericalStats.MovementSpeed);
            if (Input.GetButton("Jump"))
            {
                moveDirection.y = (float)manager.GetStat(NumericalStats.JumpHeight);
                childAnimator.SetBool(AnimJumpingHash, true);
            }
        }
        moveDirection.y -= (gravity + tempGravity) * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
        Vector3 tempDir = new Vector3(moveDirection.x, 0, moveDirection.z);
        AnimationHandler(tempDir, controller);
        if (isServer) UpdateTransform(transform.position);
    }

    void AnimationHandler(Vector3 tempDir, CharacterController controller)
    {
        if (tempDir != Vector3.zero && controller.isGrounded) 
        {
            float relativeDir = Vector3.Dot(tempDir, transform.forward);
            if (relativeDir > 0.5) childAnimator.SetInteger(AnimMovingHash, 1);
            else if (relativeDir < -0.5) childAnimator.SetInteger(AnimMovingHash, -1);
            else childAnimator.SetInteger(AnimMovingHash, 0);

            if (relativeDir > -0.6 && relativeDir < 0.6)
            {
                float relativeRightDir = Vector3.Dot(tempDir, transform.right);
                if (relativeRightDir > 0.5) childAnimator.SetInteger(AnimStrafingHash, 1);
                else if (relativeRightDir < -0.5) childAnimator.SetInteger(AnimStrafingHash, -1);
                else childAnimator.SetInteger(AnimStrafingHash, 0);
            }
            else childAnimator.SetInteger(AnimStrafingHash, 0);
        }
        else 
        {
            childAnimator.SetInteger(AnimStrafingHash, 0);
            childAnimator.SetInteger(AnimMovingHash, 0);
        }
    }

    [ClientRpc]
    void UpdateTransform(Vector3 transform)
    {
        this.transform.position = transform;
    }
}