using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class AttackManager : NetworkBehaviour
{
    [SerializeField] protected StatManager statManager;
    [SyncVar] private bool bCanAttack = true;
    [SyncVar] private bool bCanSecondary = true;
    [SyncVar] private bool bCanAbility1 = true;
    [SyncVar] private bool bCanAbility2 = true;
    [SyncVar] private bool bCanAbility3 = true;
    [SyncVar] private bool bCanAbility4 = true;

    // Update is called once per frame
    void Update()
    {
        if (!isOwned) { this.enabled = false; return; }

        if (Input.GetButtonDown("PrimaryAttack") && bCanAttack)
        {
            Debug.Log("Shot Gat");
            if (isClient) OnServerPrimaryAttack();
            else if (isServer) OnClientPrimaryAttack();
            ServerStartPrimaryAttackCooldown();
        }

        if (Input.GetButtonDown("SecondaryAttack") && bCanSecondary)
        {
            Debug.Log("Shot Gat Extra Good");
            if (isClient) OnServerSecondaryAttack();
            else if (isServer) OnClientSecondaryAttack();
            ServerStartSecondaryAttackCooldown();
        }

        if (Input.GetButtonDown("Ability1") && bCanAbility1)
        {
            Debug.Log("First Ability PewPew");
            OnServerAbility1();
        }

        if (Input.GetButtonDown("Ability2") && bCanAbility2)
        {
            Debug.Log("Second Ability PewPew");
            OnServerAbility2();
        }

        if (Input.GetButtonDown("Ability3") && bCanAbility3)
        {
            Debug.Log("Third Ability PewPew");
            OnServerAbility3();
        }

        if (Input.GetButtonDown("Ability4") && bCanAbility4)
        {
            Debug.Log("Fourth Ability PewPew");
            OnServerAbility4();
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
    /// RPC to the server to start a cooldown to enforce attack speed
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
    /// Method called on Server and all Clients after client-side input is pressed
    /// </summary>
    protected abstract void OnAbility2();

    /// <summary>
    /// Method called on Server and all Clients after client-side input is pressed
    /// </summary>
    protected abstract void OnAbility3();

    /// <summary>
    /// Method called on Server and all Clients after client-side input is pressed
    /// </summary>
    protected abstract void OnAbility4();

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
