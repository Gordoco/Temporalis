using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Handler for all player movement
/// </summary>
public class PlayerMove : NetworkBehaviour
{
    // Editor/Public values
    public float gravity = 20.0f;
    
    // Network values
    [SyncVar] private bool bDead = false;
    [SyncVar] private bool bInputDisabled = false;

    // Local values
    private float tempGravity = 0;

    private int AnimMovingHash;
    private int AnimStrafingHash;
    private int AnimJumpingHash;
    private Animator childAnimator;

    private Vector3 tempMomentum = Vector3.zero;
    private Vector3 center;
    private float radius;
    private Vector3 moveDirection = Vector3.zero;

    private bool bFlying = false;
    private bool bAwake = false;

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
        else
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
        this.center = center;
        this.radius = radius;
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

    /// <summary>
    /// Handles swing movememnt from some classes
    /// </summary>
    private void Swing()
    {
        GameObject Camera = null;
        for (int i = 0; i < gameObject.transform.childCount; i++) if (gameObject.transform.GetChild(i).tag == "MainCamera") { Camera = gameObject.transform.GetChild(i).gameObject; break; }

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

        moveDirection = dirVector * ((float)GetComponent<PlayerStatManager>().GetStat(NumericalStats.MovementSpeed) * (Mathf.Abs((loc - transform.position).magnitude) / 1.5f));
        tempMomentum = moveDirection;
        GetComponent<CharacterController>().Move(moveDirection * Time.deltaTime);
        if (isServer) UpdateTransform(transform.position);
    }

    /// <summary>
    /// Handles normal player movement and gravity
    /// </summary>
    private void Movement()
    {
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
        moveDirection.z += tempMomentum.z;

        float slowSpeed = ((float)manager.GetStat(NumericalStats.MovementSpeed) + tempMomentum.magnitude) * Time.deltaTime;

        if (tempMomentum.x > 0) tempMomentum.x -= slowSpeed;
        else if (tempMomentum.x < 0) tempMomentum.x += slowSpeed;
        if (slowSpeed >= tempMomentum.x && -slowSpeed <= tempMomentum.x) tempMomentum.x = 0;

        if (tempMomentum.z > 0) tempMomentum.z -= slowSpeed;
        else if (tempMomentum.z < 0) tempMomentum.z += slowSpeed;
        if (slowSpeed >= tempMomentum.z && -slowSpeed <= tempMomentum.z) tempMomentum.z = 0;


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

    void Update()
    {
        if (!isOwned || bDead || !bAwake) return;
        if (bInputDisabled)
        {
            Swing();
        }
        else
        {
            Movement();    
        }
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