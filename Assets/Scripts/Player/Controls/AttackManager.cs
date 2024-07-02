using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class AttackManager : NetworkBehaviour
{
    [SerializeField] private GameObject PauseMenuPrefab;
    [SerializeField] protected StatManager statManager;
    [SerializeField] private GameObject StunnedParticleEffect;
    [SerializeField, SyncVar] protected bool PrimaryFullAuto = true;
    [SerializeField, SyncVar] protected bool SecondaryFullAuto = false;
    [SyncVar] private bool bCanAttack = true;
    [SyncVar] private bool bCanSecondary = true;
    [SyncVar] private bool bCanAbility1 = true;
    [SyncVar] private bool bCanAbility2 = true;
    [SyncVar] private bool bCanAbility3 = true;
    [SyncVar] private bool bCanAbility4 = true;
    [SyncVar] private bool bEnabled = true;

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

    protected virtual void Start()
    {
        GetComponent<HitManager>().OnStunned += ShowStunnedEffect;
        GetComponent<HitManager>().OnUnStunned += HideStunnedEffect;
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

    // Update is called once per frame
    void Update()
    {
        if (!bEnabled) return;
        if (!isOwned) { this.enabled = false; return; }

        bool primaryInput = PrimaryFullAuto ? Input.GetButton("PrimaryAttack") : Input.GetButtonDown("PrimaryAttack");
        if (primaryInput && bCanAttack)
        {
            bCanAttack = false;
            Debug.Log("Shot Gat");
            if (isClient) OnServerPrimaryAttack();
            else if (isServer) OnClientPrimaryAttack();
            ServerStartPrimaryAttackCooldown();
        }


        bool secondaryInput = SecondaryFullAuto ? Input.GetButton("SecondaryAttack") : Input.GetButtonDown("SecondaryAttack");
        if (secondaryInput && bCanSecondary)
        {
            bCanSecondary = false;
            Debug.Log("Shot Gat Extra Good");
            if (isClient) OnServerSecondaryAttack();
            else if (isServer) OnClientSecondaryAttack();
            ServerStartSecondaryAttackCooldown();
        }

        //Q
        if (Input.GetButtonDown("Ability1") && bCanAbility1)
        {
            bCanAbility1 = false;
            Debug.Log("First Ability PewPew");
            if (isClient) OnServerAbility1();
            else if (isServer) OnClientAbility1();
            ServerStartAbility1Cooldown();
        }

        //E
        if (Input.GetButtonDown("Ability2") && bCanAbility2)
        {
            bCanAbility2 = false;
            Debug.Log("Second Ability PewPew");
            if (isClient) OnServerAbility2();
            else if (isServer) OnClientAbility2();
            ServerStartAbility2Cooldown();
        }

        //LEFT-CTRL
        if (Input.GetButtonDown("Ability3") && bCanAbility3)
        {
            bCanAbility3 = false;
            Debug.Log("Third Ability PewPew");
            if (isClient) OnServerAbility3();
            else if (isServer) OnClientAbility3();
            ServerStartAbility3Cooldown();
        }

        //R
        if (Input.GetButtonDown("Ability4") && bCanAbility4)
        {
            bCanAbility4 = false;
            Debug.Log("Fourth Ability PewPew");
            if (isClient) OnServerAbility4();
            else if (isServer) OnClientAbility4();
            ServerStartAbility4Cooldown();
        }

        if (Input.GetButtonDown("Pause"))
        {
            if (PauseMenuPrefab == null)
            {
                Debug.Log("ERROR: No Pause Menu Defined AttackManager");
                Debug.Break();
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
        StartCoroutine(PrimaryAttackCooldown());
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
        StartCoroutine(SecondaryAttackCooldown());
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
        StartCoroutine(Ability1Cooldown());
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
        StartCoroutine(Ability2Cooldown());
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
        StartCoroutine(Ability3Cooldown());
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
        StartCoroutine(Ability4Cooldown());
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
