using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CommandoAttack : AttackManager
{
    [SerializeField] private GameObject SecondaryAttackProjPrefab;
    [SerializeField] private GameObject PrimaryAttackParticleEffect;
    [SerializeField] private GameObject HitParticleEffect;
    [SerializeField] private GameObject GrenadePrefab;

    /// <summary>
    /// Implements the primary attack for the Commando class. Based around the existence of twin pistols
    /// that fire in tandem. Triggers two hits per shot as an intended feature, interacting with On-Hit
    /// effects.
    /// </summary>
    protected override void OnPrimaryAttack()
    {
        GameObject Weapon = null;
        for (int i = 0; i < gameObject.transform.childCount; i++) if (gameObject.transform.GetChild(i).tag == "Weapon") { Weapon = gameObject.transform.GetChild(i).gameObject; break; }
        GameObject Camera = null;
        for (int i = 0; i < gameObject.transform.childCount; i++) if (gameObject.transform.GetChild(i).tag == "MainCamera") { Camera = gameObject.transform.GetChild(i).gameObject; break; }

        StartCoroutine(WeaponSwell(Weapon.transform.GetChild(0).gameObject, statManager.GetStat(NumericalStats.AttackSpeed)));
        StartCoroutine(WeaponSwell(Weapon.transform.GetChild(1).gameObject, statManager.GetStat(NumericalStats.AttackSpeed)));

        if (isServer) //Server Side Only
        {
            Vector3 start1 = Camera.transform.position;
            Vector3 start2 = start1;

            Vector3 dir = Camera.transform.forward;
            dir.Normalize();

            RaycastHit hit1;
            RaycastHit hit2;

            Vector3 Gun1MuzzleLoc = Weapon.transform.GetChild(0).position + (dir * 0.5f);
            Vector3 Gun2MuzzleLoc = Weapon.transform.GetChild(1).position + (dir * 0.5f);

            GameObject MF1 = Instantiate(PrimaryAttackParticleEffect, Gun1MuzzleLoc, Quaternion.LookRotation(dir));
            GameObject MF2 = Instantiate(PrimaryAttackParticleEffect, Gun2MuzzleLoc, Quaternion.LookRotation(dir));
            NetworkServer.Spawn(MF1);
            NetworkServer.Spawn(MF2);

            if (Physics.Raycast(start1, dir, out hit1, (float)statManager.GetStat(NumericalStats.Range))) { /*Debug.Log("GUN 1 HIT OBJECT");*/ }
            if (Physics.Raycast(start2, dir, out hit2, (float)statManager.GetStat(NumericalStats.Range))) { /*Debug.Log("GUN 2 HIT OBJECT");*/ }

            if (hit1.collider != null && hit1.collider.gameObject != null && hit1.collider.gameObject.GetComponent<HitManager>() != null) 
            { 
                hit1.collider.gameObject.GetComponent<HitManager>().Hit((float)statManager.GetStat(NumericalStats.PrimaryDamage));
                GameObject HP1 = Instantiate(HitParticleEffect, hit1.transform.position, Quaternion.LookRotation(dir));
                NetworkServer.Spawn(HP1);
            }
            if (hit2.collider != null && hit2.collider.gameObject != null && hit2.collider.gameObject.GetComponent<HitManager>() != null) 
            { 
                hit2.collider.gameObject.GetComponent<HitManager>().Hit((float)statManager.GetStat(NumericalStats.PrimaryDamage));
                GameObject HP2 = Instantiate(HitParticleEffect, hit2.transform.position, Quaternion.LookRotation(dir));
                NetworkServer.Spawn(HP2);
            }
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
        //Debug.Log("Started Swell");
        float swellSpeed = 0.02f;
        for (int i = 0; i < 12; i++)
        {
            Weapon.transform.localScale += new Vector3(swellSpeed, swellSpeed, swellSpeed);
            //Debug.Log(gameObject.name + " " + 0.01f / (float)AttackSpeed);
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
        if (isServer && GrenadePrefab != null)
        {
            GameObject Camera = null;
            for (int i = 0; i < gameObject.transform.childCount; i++) if (gameObject.transform.GetChild(i).tag == "MainCamera") { Camera = gameObject.transform.GetChild(i).gameObject; break; }

            GameObject proj = Instantiate(GrenadePrefab);
            if (proj.GetComponent<ProjectileCreator>() == null)
            {
                Destroy(proj);
                return;
            }
            float forwardOffset = 5;
            proj.GetComponent<ProjectileCreator>().InitializeProjectile(gameObject, Camera.transform.position + (Camera.transform.forward * forwardOffset), Camera.transform.forward, 0);
            proj.GetComponent<GrenadeTimedExplosion>().Init(gameObject, 20, (float)statManager.GetStat(NumericalStats.SecondaryDamage));
        }
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

//OLD SECONDARY ATTACK
/*
if (isServer && SecondaryAttackProjPrefab != null)
{
    Debug.Log(statManager.GetStat(NumericalStats.SecondaryCooldown));
    GameObject Weapon = null;
    for (int i = 0; i < gameObject.transform.childCount; i++) if (gameObject.transform.GetChild(i).tag == "Weapon") { Weapon = gameObject.transform.GetChild(i).gameObject; break; }

    GameObject Camera = null;
    for (int i = 0; i < gameObject.transform.childCount; i++) if (gameObject.transform.GetChild(i).tag == "MainCamera") { Camera = gameObject.transform.GetChild(i).gameObject; break; }

    GameObject proj = Instantiate(SecondaryAttackProjPrefab);

    //Handle Bad Prefab/Weapon
    if (proj.GetComponent<ProjectileCreator>() == null || Weapon == null)
    {
        Destroy(proj);
        return;
    }
    float forwardOffset = 5;
    proj.GetComponent<ProjectileCreator>().InitializeProjectile(gameObject, Camera.transform.position + (Camera.transform.forward * forwardOffset), Camera.transform.forward, statManager.GetStat(NumericalStats.SecondaryDamage));
}
*/
