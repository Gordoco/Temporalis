using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CommandoAttack : AttackManager
{ 
    protected override void OnPrimaryAttack()
    {
        GameObject Weapon = null;
        for (int i = 0; i < gameObject.transform.childCount; i++) if (gameObject.transform.GetChild(i).tag == "Weapon") { Weapon = gameObject.transform.GetChild(i).gameObject; break; }
        StartCoroutine(WeaponSwell(Weapon.transform.GetChild(0).gameObject, statManager.GetStat(NumericalStats.AttackSpeed)));
        StartCoroutine(WeaponSwell(Weapon.transform.GetChild(1).gameObject, statManager.GetStat(NumericalStats.AttackSpeed)));
        statManager.SetStat(NumericalStats.Range, 100); //BAD Move to stat initialization later
        Debug.DrawLine(Weapon.transform.GetChild(0).position, Weapon.transform.GetChild(0).position + (Weapon.transform.GetChild(0).forward * (float)statManager.GetStat(NumericalStats.Range)), Color.red, 1 / (float)statManager.GetStat(NumericalStats.AttackSpeed));
        Debug.DrawLine(Weapon.transform.GetChild(1).position, Weapon.transform.GetChild(1).position + (Weapon.transform.GetChild(1).forward * (float)statManager.GetStat(NumericalStats.Range)), Color.red, 1 / (float)statManager.GetStat(NumericalStats.AttackSpeed));

        Vector3 start1 = Weapon.transform.GetChild(0).position;
        Vector3 start2 = Weapon.transform.GetChild(1).position;
        Vector3 dir = Weapon.transform.GetChild(0).forward;
        dir.Normalize();
        RaycastHit hit1;
        RaycastHit hit2;

        Physics.Raycast(start1, dir, out hit1, (float)statManager.GetStat(NumericalStats.Range));
        Physics.Raycast(start2, dir, out hit2, (float)statManager.GetStat(NumericalStats.Range));

        if (isServer) //Server Side Only
        {
            if (hit1.collider.gameObject.GetComponent<HitManager>()) hit1.collider.gameObject.GetComponent<HitManager>().Hit((float)statManager.GetStat(NumericalStats.Damage));
            if (hit1.collider.gameObject.GetComponent<HitManager>()) hit1.collider.gameObject.GetComponent<HitManager>().Hit((float)statManager.GetStat(NumericalStats.Damage));
        }
    }

    private IEnumerator WeaponSwell(GameObject Weapon, double AttackSpeed)
    {
        float swellSpeed = 0.02f / (float)AttackSpeed;
        for (int i = 0; i < 12; i++)
        {
            Weapon.transform.localScale += new Vector3(swellSpeed, swellSpeed, swellSpeed);
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
