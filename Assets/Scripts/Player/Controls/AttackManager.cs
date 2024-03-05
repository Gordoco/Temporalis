using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class AttackManager : NetworkBehaviour
{
    [SerializeField] protected StatManager statManager;
    [SyncVar] private bool bCanAttack = true;

    // Update is called once per frame
    void Update()
    {
        if (!isOwned) { this.enabled = false; return; }

        if (Input.GetButtonDown("PrimaryAttack") && bCanAttack)
        {
            Debug.Log("Shot Gat");
            if (!isServer) OnServerPrimaryAttack();
            else OnClientPrimaryAttack();
            ServerStartPrimaryAttackCooldown();
        }

        if (Input.GetButtonDown("SecondaryAttack"))
        {
            Debug.Log("Shot Gat Extra Good");
            OnServerSecondaryAttack();
        }

        if (Input.GetButtonDown("Ability1"))
        {
            Debug.Log("First Ability PewPew");
            OnServerAbility1();
        }

        if (Input.GetButtonDown("Ability2"))
        {
            Debug.Log("Second Ability PewPew");
            OnServerAbility2();
        }

        if (Input.GetButtonDown("Ability3"))
        {
            Debug.Log("Third Ability PewPew");
            OnServerAbility3();
        }

        if (Input.GetButtonDown("Ability4"))
        {
            Debug.Log("Fourth Ability PewPew");
            OnServerAbility4();
        }
    }

    /*
     * TODO:
     * Do this for all types of abilities/attacks
     */
    [Command]
    private void ServerStartPrimaryAttackCooldown()
    {
        bCanAttack = false;
        StartCoroutine(PrimaryAttackCooldown());
    }

    private IEnumerator PrimaryAttackCooldown()
    {
        yield return new WaitForSeconds(1/(float)statManager.GetStat(NumericalStats.AttackSpeed));
        bCanAttack = true;
    }

    /// <summary>
    /// Method called on Server and all Clients after client-side input is pressed
    /// </summary>
    protected abstract void OnPrimaryAttack();

    /// <summary>
    /// Method called on Server and all Clients after client-side input is pressed
    /// </summary>
    protected abstract void OnSecondaryAttack();

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
    protected void OnClientSecondaryAttack() { OnPrimaryAttack(); }

    [ClientRpc]
    protected void OnClientAbility1() { OnPrimaryAttack(); }

    [ClientRpc]
    protected void OnClientAbility2() { OnPrimaryAttack(); }

    [ClientRpc]
    protected void OnClientAbility3() { OnPrimaryAttack(); }

    [ClientRpc]
    protected void OnClientAbility4() { OnPrimaryAttack(); }

    [Command]
    protected virtual void OnServerPrimaryAttack() { OnPrimaryAttack(); OnClientPrimaryAttack(); }

    [Command]
    protected virtual void OnServerSecondaryAttack() { OnSecondaryAttack(); OnClientSecondaryAttack(); }

    [Command]
    protected virtual void OnServerAbility1() { OnAbility1(); OnClientAbility1(); }

    [Command]
    protected virtual void OnServerAbility2() { OnAbility2(); OnClientAbility2(); }

    [Command]
    protected virtual void OnServerAbility3() { OnAbility3(); OnClientAbility3(); }

    [Command]
    protected virtual void OnServerAbility4() { OnAbility4(); OnClientAbility4(); }
}
