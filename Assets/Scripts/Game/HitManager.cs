using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(StatManager))]
public class HitManager : NetworkBehaviour
{
    private StatManager manager;

    public event System.EventHandler OnUnStunned;
    public event System.EventHandler OnStunned;

    private void Start()
    {
        if (isServer)
            manager = GetComponent<StatManager>();
    }

    /// <summary>
    /// Server-Only method which modifies the requested entity's health value
    /// </summary>
    /// <param name="damage"> Value to be subtracted from the health of the owning entity </param>
    /// <returns> True if entity died, false otherwise </returns>
    [Server]
    public virtual bool Hit(double damage)
    {
        //Debug.Log(gameObject.name + " has been hit for: " + damage + " damage. Has: " + ((float)manager.GetHealth() - damage) + " Health");
        manager.DealDamage(damage);
        if (manager.GetHealth() <= 0) return true;
        return false;
    }

    /// <summary>
    /// Server-Only method which stuns the selected entity, preventing actions
    /// </summary>
    [Server]
    public virtual void Stun(float time)
    {
        if (manager.GetCCImmune()) return;
        GetComponent<CharacterController>().enabled = false; //Stop Movement
        Client_ToggleController(false);
        OnStunned.Invoke(this, null);
        if (GetComponent<PlayerStatManager>())
        {
            //If Player, Disable Attack Manager
            GetComponent<AttackManager>().SetEnabled(false);
        }
        StartCoroutine(UnStun(time));
    }

    [ClientRpc]
    private void Client_ToggleController(bool b)
    {
        GetComponent<CharacterController>().enabled = b;
        if (GetComponent<PlayerStatManager>())
        {
            //If Player, Disable Attack Manager
            GetComponent<AttackManager>().SetEnabled(b);
        }
    }

    private IEnumerator UnStun(float time)
    {
        yield return new WaitForSeconds(time);
        GetComponent<CharacterController>().enabled = true; //Start Movement
        Client_ToggleController(true);
        OnUnStunned.Invoke(this, null);
        if (GetComponent<PlayerStatManager>())
        {
            //If Player, Enable Attack Manager
            GetComponent<AttackManager>().SetEnabled(true);
        }
    }
}
