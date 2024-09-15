using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Abstract interface for handling the network resolution of player controls
/// </summary>
[RequireComponent(typeof(PredictionHandler))]
public abstract class AttackManager : NetworkBehaviour
{
    // Editor values
    [SerializeField] private GameObject PauseMenuPrefab;
    [SerializeField] protected StatManager statManager;
    [SerializeField] private GameObject StunnedParticleEffect;
    [SerializeField] private float Gravity = 1f;
    [SerializeField] private float mouseXSensitivity = 100f;
    [SerializeField] private float mouseYSensitivity = 1f;
    [SerializeField] private GameObject PlayerBody;
    [SerializeField] private GameObject PlayerCamera;
    [SerializeField] private View TopDownView = new View(new Vector3(0, 3, -2), new Vector3(52, 0, 0));
    [SerializeField] private View StraightView = new View(new Vector3(0, 2.23f, -4.18f), new Vector3(10, 0, 0));
    [SerializeField] private View DownTopView = new View(new Vector3(0, -0.27f, -1), new Vector3(-70, 0, 0));

    private float yRotation = 0.3f;

    private Vector3 moveDirection;

    // Editor network values
    [SerializeField, SyncVar] protected bool bIgnorePrimaryCooldown = false;
    [SerializeField, SyncVar] protected bool PrimaryFullAuto = true;
    [SerializeField, SyncVar] protected bool SecondaryFullAuto = false;
    [SerializeField, SyncVar] protected bool bIgnoreAbility1Cooldown = false;
    [SerializeField, SyncVar] protected bool Ability1FullAuto = false;
    [SerializeField, SyncVar] protected bool Ability2FullAuto = false;
    [SerializeField, SyncVar] protected bool Ability3FullAuto = false;
    [SerializeField, SyncVar] protected bool Ability4FullAuto = false;

    // Local network values
    [SyncVar] protected bool bCanAttack = true;
    [SyncVar] protected bool bCanSecondary = true;
    [SyncVar] protected bool bCanAbility1 = true;
    [SyncVar] protected bool bCanAbility2 = true;
    [SyncVar] protected bool bCanAbility3 = true;
    [SyncVar] protected bool bCanAbility4 = true;
    [SyncVar] protected bool bEnabled = true;

    protected Coroutine PrimaryCooldownCoroutine;
    protected Coroutine SecondaryCooldownCoroutine;
    protected Coroutine Ability1CooldownCoroutine;
    protected Coroutine Ability2CooldownCoroutine;
    protected Coroutine Ability3CooldownCoroutine;
    protected Coroutine Ability4CooldownCoroutine;

    // Local values
    private GameObject PauseMenu;

    public bool GetAbilityReady(int abilityNum)
    {
        switch (abilityNum)
        {
            case 0:
                return bCanAttack;
            case 1:
                return bCanSecondary;
            case 2:
                return bCanAbility1;
            case 3:
                return bCanAbility2;
            case 4:
                return bCanAbility3;
            case 5:
                return bCanAbility4;
            default:
                return false;
        }
    }

    public void SetEnabled(bool b) { bEnabled = b; }

    // TODO: Post Status-Effect implementation, convert to a more generic solution
    protected virtual void Start()
    {
        GetComponent<HitManager>().OnStunned += ShowStunnedEffect;
        GetComponent<HitManager>().OnUnStunned += HideStunnedEffect;

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void ShowStunnedEffect(object sender, System.EventArgs e)
    {
        if (StunnedParticleEffect) StunnedParticleEffect.SetActive(true);
        ClientShowStunned(true);
    }
    private void HideStunnedEffect(object sender, System.EventArgs e)
    {
        if (StunnedParticleEffect) StunnedParticleEffect.SetActive(false);
        ClientShowStunned(false);
    }

    [ClientRpc]
    private void ClientShowStunned(bool b)
    {
        if (StunnedParticleEffect) StunnedParticleEffect.SetActive(b);
    }

    bool b = true;
    // Update is called once per frame
    protected virtual void Update()
    {
        if (!bEnabled) return;
        if (!isOwned) { this.enabled = false; return; }

        if (isClient && b) Debug.Log(transform.name + ": Is Running an Attack Manager");
        b = false;

        //Default: LMB
        bool primaryInput = PrimaryFullAuto ? Input.GetButton("PrimaryAttack") : Input.GetButtonDown("PrimaryAttack");
        if (primaryInput && bCanAttack)
        {
            if (!bIgnorePrimaryCooldown) bCanAttack = false;
            if (isClient) OnServerPrimaryAttack();
            else if (isServer) OnClientPrimaryAttack();
            if (!bIgnorePrimaryCooldown) ServerStartPrimaryAttackCooldown();
        }

        //Default: RMB
        bool secondaryInput = SecondaryFullAuto ? Input.GetButton("SecondaryAttack") : Input.GetButtonDown("SecondaryAttack");
        if (secondaryInput && bCanSecondary)
        {
            bCanSecondary = false;
            if (isClient) OnServerSecondaryAttack();
            else if (isServer) OnClientSecondaryAttack();
            ServerStartSecondaryAttackCooldown();
        }

        //Default: Q
        bool ability1Input = Ability1FullAuto ? Input.GetButton("Ability1") : Input.GetButtonDown("Ability1");
        if (ability1Input && bCanAbility1)
        {
            bCanAbility1 = false;
            if (isClient) OnServerAbility1();
            else if (isServer) OnClientAbility1();
            if (!bIgnoreAbility1Cooldown) ServerStartAbility1Cooldown();
        }

        //Default: E
        bool ability2Input = Ability2FullAuto ? Input.GetButton("Ability2") : Input.GetButtonDown("Ability2");
        if (ability2Input && bCanAbility2)
        {
            bCanAbility2 = false;
            if (isClient) OnServerAbility2();
            else if (isServer) OnClientAbility2();
            ServerStartAbility2Cooldown();
        }

        //Default: LEFT-CTRL
        bool ability3Input = Ability3FullAuto ? Input.GetButton("Ability3") : Input.GetButtonDown("Ability3");
        if (ability3Input && bCanAbility3)
        {
            bCanAbility3 = false;
            if (isClient) OnServerAbility3();
            else if (isServer) OnClientAbility3();
            ServerStartAbility3Cooldown();
        }

        //Default: R
        bool ability4Input = Ability4FullAuto ? Input.GetButton("Ability4") : Input.GetButtonDown("Ability4");
        if (ability4Input && bCanAbility4)
        {
            bCanAbility4 = false;
            if (isClient) OnServerAbility4();
            else if (isServer) OnClientAbility4();
            ServerStartAbility4Cooldown();
        }

        //Default: ESC
        if (Input.GetButtonDown("Pause"))
        {
            if (PauseMenuPrefab == null)
            {
                Debug.Log("[ERROR - AttackManager.cs: No Pause Menu Defined]");
                Debug.Break(); // Stops editor play "Freezing" game
                return;
            }

            if (PauseMenu == null)
            {
                PauseMenu = Instantiate(PauseMenuPrefab);
                PauseMenu.GetComponentInChildren<DisconnectHandler>().isServer = isServer;
                PauseMenu.transform.SetParent(transform);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Destroy(PauseMenu);
                PauseMenu = null;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        if (isClient)
        {
            HandleMovement();
            //HandleJump();
            //HandleLook();
        }
    }

    protected virtual void HandleMovement()
    {
        PredictionHandler predictionHandler = GetComponent<PredictionHandler>();

        moveDirection.x = Input.GetAxis("Horizontal");
        moveDirection.y -= Gravity;
        moveDirection.z = Input.GetAxis("Vertical");

        Vector3 dir = transform.TransformDirection(new Vector3(moveDirection.x, 0, moveDirection.z).normalized) * (float)statManager.GetStat(NumericalStats.MovementSpeed);
        dir.y = moveDirection.y;

        predictionHandler.ProcessTranslation(dir);
    }

    protected virtual void HandleLook()
    {
        PredictionHandler predictionHandler = GetComponent<PredictionHandler>();
        PredictionHandler cameraPredictionHandler = PlayerCamera.GetComponent<PredictionHandler>();

        float mouseX = Input.GetAxis("Mouse X") * mouseXSensitivity * predictionHandler.GetMinTimeBetweenTicks();
        float mouseY = -1 * Input.GetAxis("Mouse Y") * mouseYSensitivity * predictionHandler.GetMinTimeBetweenTicks();

        yRotation = Mathf.Clamp(yRotation + mouseY, 0, 1); Vector3 pos;

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

        //X Rotation
        Quaternion newRot = Quaternion.Euler(transform.rotation.eulerAngles + Vector3.up * mouseX);

        predictionHandler.ProcessRotation(newRot);
        cameraPredictionHandler.ProcessTranslation(pos);
        cameraPredictionHandler.ProcessRotation(rot);
    }

    protected virtual void HandleJump()
    {
        bool bJump = Input.GetButton("Jump");

        if (GetComponent<CharacterController>().isGrounded)
        {
            if (bJump) moveDirection.y = (float)statManager.GetStat(NumericalStats.JumpHeight);
            else moveDirection.y = 0;
        }
    }

    /// <summary>
    /// Method called on Server and all Clients after client-side input is pressed
    /// </summary>
    protected abstract void OnPrimaryAttack();

    /// <summary>
    /// RPC to the server to start a cooldown to enforce attack speed
    /// Updates Server-Only Variables for network security.
    /// </summary>
    [Command]
    private void ServerStartPrimaryAttackCooldown()
    {
        bCanAttack = false;
        PrimaryCooldownCoroutine = StartCoroutine(PrimaryAttackCooldown());
    }

    private IEnumerator PrimaryAttackCooldown()
    {
        yield return new WaitForSeconds(1 / (float)statManager.GetStat(NumericalStats.AttackSpeed));
        bCanAttack = true;
    }

    /// <summary>
    /// Method called on Server and all Clients after client-side input is pressed
    /// </summary>
    protected abstract void OnSecondaryAttack();

    /// <summary>
    /// RPC to the server to start a cooldown to enforce cooldown
    /// Updates Server-Only Variables for network security.
    /// </summary>
    [Command]
    private void ServerStartSecondaryAttackCooldown()
    {
        bCanSecondary = false;
        SecondaryCooldownCoroutine = StartCoroutine(SecondaryAttackCooldown());
    }

    private IEnumerator SecondaryAttackCooldown()
    {
        yield return new WaitForSeconds((float)statManager.GetStat(NumericalStats.SecondaryCooldown));
        bCanSecondary = true;
    }

    /// <summary>
    /// Method called on Server and all Clients after client-side input is pressed
    /// </summary>
    protected abstract void OnAbility1();

    /// <summary>
    /// RPC to the server to start a cooldown to enforce cooldown
    /// Updates Server-Only Variables for network security.
    /// </summary>
    [Command]
    private void ServerStartAbility1Cooldown()
    {
        bCanAbility1 = false;
        Ability1CooldownCoroutine = StartCoroutine(Ability1Cooldown());
    }

    private IEnumerator Ability1Cooldown()
    {
        yield return new WaitForSeconds((float)statManager.GetStat(NumericalStats.Ability1Cooldown));
        bCanAbility1 = true;
    }

    /// <summary>
    /// Method called on Server and all Clients after client-side input is pressed
    /// </summary>
    protected abstract void OnAbility2();

    /// <summary>
    /// RPC to the server to start a cooldown to enforce cooldown
    /// Updates Server-Only Variables for network security.
    /// </summary>
    [Command]
    private void ServerStartAbility2Cooldown()
    {
        bCanAbility2 = false;
        Ability2CooldownCoroutine = StartCoroutine(Ability2Cooldown());
    }

    private IEnumerator Ability2Cooldown()
    {
        yield return new WaitForSeconds((float)statManager.GetStat(NumericalStats.Ability2Cooldown));
        bCanAbility2 = true;
    }

    /// <summary>
    /// Method called on Server and all Clients after client-side input is pressed
    /// </summary>
    protected abstract void OnAbility3();

    /// <summary>
    /// RPC to the server to start a cooldown to enforce cooldown
    /// Updates Server-Only Variables for network security.
    /// </summary>
    [Command]
    private void ServerStartAbility3Cooldown()
    {
        bCanAbility3 = false;
        Ability3CooldownCoroutine = StartCoroutine(Ability3Cooldown());
    }

    private IEnumerator Ability3Cooldown()
    {
        yield return new WaitForSeconds((float)statManager.GetStat(NumericalStats.Ability3Cooldown));
        bCanAbility3 = true;
    }

    /// <summary>
    /// Method called on Server and all Clients after client-side input is pressed
    /// </summary>
    protected abstract void OnAbility4();

    /// <summary>
    /// RPC to the server to start a cooldown to enforce cooldown
    /// Updates Server-Only Variables for network security.
    /// </summary>
    [Command]
    private void ServerStartAbility4Cooldown()
    {
        bCanAbility4 = false;
        Ability4CooldownCoroutine = StartCoroutine(Ability4Cooldown());
    }

    private IEnumerator Ability4Cooldown()
    {
        yield return new WaitForSeconds((float)statManager.GetStat(NumericalStats.Ability4Cooldown));
        bCanAbility4 = true;
    }

    [ClientRpc]
    protected void OnClientPrimaryAttack() { OnPrimaryAttack(); }

    [ClientRpc]
    protected void OnClientSecondaryAttack() { OnSecondaryAttack(); }

    [ClientRpc]
    protected void OnClientAbility1() { OnAbility1(); }

    [ClientRpc]
    protected void OnClientAbility2() { OnAbility2(); }

    [ClientRpc]
    protected void OnClientAbility3() { OnAbility3(); }

    [ClientRpc]
    protected void OnClientAbility4() { OnAbility4(); }

    [Command]
    protected virtual void OnServerPrimaryAttack() { OnClientPrimaryAttack(); }

    [Command]
    protected virtual void OnServerSecondaryAttack() { OnClientSecondaryAttack(); }

    [Command]
    protected virtual void OnServerAbility1() { OnClientAbility1(); }

    [Command]
    protected virtual void OnServerAbility2() { OnClientAbility2(); }

    [Command]
    protected virtual void OnServerAbility3() { OnClientAbility3(); }

    [Command]
    protected virtual void OnServerAbility4() { OnClientAbility4(); }
}
