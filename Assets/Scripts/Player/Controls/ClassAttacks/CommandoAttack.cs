using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CommandoAttack : AttackManager
{ 
    /// <summary>
    /// Implements the primary attack for the Commando class. Based around the existence of twin pistols
    /// that fire in tandem. Triggers two hits per shot as an intended feature, interacting with On-Hit
    /// effects.
    /// </summary>
    protected override void OnPrimaryAttack()
    {
        GameObject Weapon = null;
        for (int i = 0; i < gameObject.transform.childCount; i++) if (gameObject.transform.GetChild(i).tag == "Weapon") { Weapon = gameObject.transform.GetChild(i).gameObject; break; }
        StartCoroutine(WeaponSwell(Weapon.transform.GetChild(0).gameObject, statManager.GetStat(NumericalStats.AttackSpeed)));
        StartCoroutine(WeaponSwell(Weapon.transform.GetChild(1).gameObject, statManager.GetStat(NumericalStats.AttackSpeed)));

        if (isServer) //Server Side Only
        {
            Vector3 start1 = Weapon.transform.position;
            Vector3 start2 = start1;
            Vector3 dir = Weapon.transform.forward;
            dir.Normalize();
            RaycastHit hit1;
            RaycastHit hit2;


            if (Physics.Raycast(start1, dir, out hit1, (float)statManager.GetStat(NumericalStats.Range))) { Debug.Log("GUN 1 HIT OBJECT"); }
            if (Physics.Raycast(start2, dir, out hit2, (float)statManager.GetStat(NumericalStats.Range))) { Debug.Log("GUN 2 HIT OBJECT"); }

            if (hit1.collider != null && hit1.collider.gameObject != null && hit1.collider.gameObject.GetComponent<HitManager>() != null) { hit1.collider.gameObject.GetComponent<HitManager>().Hit((float)statManager.GetStat(NumericalStats.Damage)); }
            if (hit2.collider != null && hit2.collider.gameObject != null && hit2.collider.gameObject.GetComponent<HitManager>() != null) { hit2.collider.gameObject.GetComponent<HitManager>().Hit((float)statManager.GetStat(NumericalStats.Damage)); }
        }
    }

    /// <summary>
    /// Template method for showcasing in a visual way a Weapon-Fired event
    /// </summary>
    /// <param name="Weapon">The GameObject which owns all weapon models (can be the sole model)</param>
    /// <param name="AttackSpeed">Passed in parameter corresponding to the AttackSpeed stat of the entity</param>
    /// <returns></returns>
    private IEnumerator WeaponSwell(GameObject Weapon, double AttackSpeed)
    {
        Debug.Log("Started Swell");
        float swellSpeed = 0.02f;
        for (int i = 0; i < 12; i++)
        {
            Weapon.transform.localScale += new Vector3(swellSpeed, swellSpeed, swellSpeed);
            Debug.Log(gameObject.name + " " + 0.01f / (float)AttackSpeed);
            yield return new WaitForSeconds(0.01f / (float)AttackSpeed);
        }
        for (int i = 0; i < 12; i++)
        {
            Weapon.transform.localScale += new Vector3(-swellSpeed, -swellSpeed, -swellSpeed);
            yield return new WaitForSeconds(0.01f / (float)AttackSpeed);
        }
        yield return null;
    }

    protected override void OnSecondaryAttack()
    {

    }

    protected override void OnAbility1()
    {

    }

    protected override void OnAbility2()
    {

    }

    protected override void OnAbility3()
    {

    }

    protected override void OnAbility4()
    {

    }
}
