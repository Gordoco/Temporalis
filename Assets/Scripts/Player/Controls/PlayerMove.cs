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
    private bool bInputDisabled = false;
    private float tempGravity = 0;
    private int AnimMovingHash;
    private int AnimStrafingHash;
    private int AnimJumpingHash;
    Animator childAnimator;
    private Vector2 tempMomentum = Vector2.zero;

    private void Awake()
    {
        AnimMovingHash = Animator.StringToHash("Vertical");
        AnimStrafingHash = Animator.StringToHash("Horizontal");
        AnimJumpingHash = Animator.StringToHash("Jumping");
        childAnimator = GetComponentInChildren<Animator>();
    }

    public void SetFlying(bool b)
    {
        if (b)
        {
            if (childAnimator) childAnimator.SetBool(AnimJumpingHash, true);
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

    [Server]
    public void Server_StopSwing()
    {
        Client_StopSwing();
    }

    private void Client_StopSwing()
    {
        bInputDisabled = false;
    }

    [Server]
    public void Server_Swing(Vector3 center, float radius)
    {
        Client_Swing(center, radius);
    }

    private void Client_Swing(Vector3 center, float radius)
    {
        bInputDisabled = true;
        GameObject Camera = null;
        for (int i = 0; i < gameObject.transform.childCount; i++) if (gameObject.transform.GetChild(i).tag == "MainCamera") { Camera = gameObject.transform.GetChild(i).gameObject; break; }
        //Debug.DrawLine(transform.position, Camera.transform.forward * 1000, Color.green);
        
        Vector3 forward = Camera.transform.forward * (float)GetComponent<PlayerStatManager>().GetStat(NumericalStats.MovementSpeed);
        Vector3 radialVector = (forward + transform.position - center).normalized;
        Vector3 loc = center + (radialVector * radius);
        Vector3 dirVector;
        if (Vector3.Distance(forward + transform.position, center) > Vector3.Distance(loc, center))
        {
            dirVector = (loc - transform.position).normalized;
        }
        else
        {
            dirVector = forward.normalized;
        }
        //Debug.DrawLine(transform.position, (loc - transform.position).normalized * 1000, Color.red);
        moveDirection = dirVector * ((float)GetComponent<PlayerStatManager>().GetStat(NumericalStats.MovementSpeed) * (Mathf.Abs((loc - transform.position).magnitude)/1.5f));
        tempMomentum = new Vector2(moveDirection.x, moveDirection.y);
        GetComponent<CharacterController>().Move(moveDirection * Time.deltaTime);
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
        if (!isOwned || bDead || !bAwake || bInputDisabled) return;

        CharacterController controller = GetComponent<CharacterController>();
        if (!controller.enabled) return;
        StatManager manager = GetComponent<StatManager>();

        Vector2 moveDirection2D = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;

        float moveDirectionY = moveDirection.y;
        moveDirection = new Vector3(moveDirection2D.x, 0, moveDirection2D.y);
        moveDirection = transform.TransformDirection(moveDirection);
        moveDirection *= (float)manager.GetStat(NumericalStats.MovementSpeed);
        moveDirection.y = moveDirectionY;

        moveDirection.x += tempMomentum.x;
        moveDirection.z += tempMomentum.y;

        float slowSpeed = ((float)manager.GetStat(NumericalStats.MovementSpeed) + tempMomentum.magnitude) * Time.deltaTime;

        if (tempMomentum.x > 0) tempMomentum.x -= slowSpeed;
        else if (tempMomentum.x < 0) tempMomentum.x += slowSpeed;
        if (slowSpeed >= tempMomentum.x && -slowSpeed <= tempMomentum.x) tempMomentum.x = 0;

        if (tempMomentum.y > 0) tempMomentum.y -= slowSpeed;
        else if (tempMomentum.y < 0) tempMomentum.y += slowSpeed;
        if (slowSpeed >= tempMomentum.y && -slowSpeed <= tempMomentum.y) tempMomentum.y = 0;


        if (controller.isGrounded && !bFlying)
        {
            if (childAnimator) childAnimator.SetBool(AnimJumpingHash, false);
            tempGravity = 0;
            moveDirection.y = 0;
            if (Input.GetButton("Jump"))
            {
                moveDirection.y = (float)manager.GetStat(NumericalStats.JumpHeight);
                if (childAnimator) childAnimator.SetBool(AnimJumpingHash, true);
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
        if (childAnimator) childAnimator.SetFloat(AnimMovingHash, Input.GetAxis("Vertical"));
        if (childAnimator) childAnimator.SetFloat(AnimStrafingHash, Input.GetAxis("Horizontal"));
    }

    [ClientRpc]
    void UpdateTransform(Vector3 transform)
    {
        this.transform.position = transform;
    }
}