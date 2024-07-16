using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmManager : NetworkBehaviour
{
    [SerializeField] private float travelSpeed = 0.5f;

    private GameObject Owner;
    private PlayerStatManager Manager;
    private bool bActive = false;

    private bool bCanAttack = true;
    private Quaternion initRotation = Quaternion.identity;
    private Vector3 initLocation = Vector3.zero;

    [Server]
    public void Init(GameObject owner)
    {
        initLocation = transform.localPosition;
        initRotation = transform.localRotation;
        Owner = owner;
        Manager = owner.GetComponent<PlayerStatManager>();
        travelSpeed = (float)Manager.GetStat(NumericalStats.AttackSpeed) * 25;
        if (!Manager) Debug.LogError("ERROR - [ArmManager.cs - Attempted to initialize an arm on non-player]");
        ToggleActive(true);
    }

    [Server]
    public void ToggleActive(bool newActive)
    {
        bActive = newActive;
    }

    [Server]
    public bool GetActive()
    {
        return bActive;
    }

    // Update is called once per frame
    void Update()
    {
        //Server only script
        if (!isServer) return;
        if (bActive)
        {
            AttackHandler();
            AmbientMovementHandler();
        }
        else
        {
            ResetActive();
        }
    }

    private void ResetActive()
    {
        transform.localPosition = initLocation;
    }

    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(1 / (float) Manager.GetStat(NumericalStats.AttackSpeed));
        bCanAttack = true;
    }

    private void AttackHandler()
    {
        if (!bCanAttack) return;
        bCanAttack = false;
        StartCoroutine(AttackCooldown());

        float currShortest = float.MaxValue;
        GameObject currNearest = null;
        GameObject[] Enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < Enemies.Length; i++)
        {
            float dist = Vector3.Distance(gameObject.transform.position, Enemies[i].transform.position);
            if (dist < currShortest)
            {
                currShortest = dist;
                currNearest = Enemies[i];
            }
        }

        if (currNearest == null) return;

        if (currShortest <= (float)Manager.GetStat(NumericalStats.Range))
        {
            if (!currNearest.GetComponent<StatManager>()) return;
            MakeAttack(currNearest);
        }
    }

    private void MakeAttack(GameObject enemy)
    {
        if (!attackInProg) StartCoroutine(TravelToAttack(enemy));
    }

    bool attackInProg = false;
    private IEnumerator TravelToAttack(GameObject enemy)
    {
        yield return new WaitForSeconds(Random.Range(0, 0.3f));
        attackInProg = true;
        float prog = 0;
        Vector3 endLocation = enemy ? transform.InverseTransformPoint(enemy.transform.position) : initLocation;
        float speed = Time.deltaTime * (travelSpeed/Vector3.Distance(initLocation, endLocation));
        while (prog < 1)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            if (enemy && enemy.GetComponent<StatManager>().GetHealth() > 0) endLocation = transform.InverseTransformPoint(enemy.transform.position);
            speed = Vector3.Distance(initLocation, endLocation) != 0 ? Time.deltaTime * (travelSpeed / Vector3.Distance(initLocation, endLocation)) : Time.deltaTime * travelSpeed;
            prog += speed;
            transform.localPosition = Vector3.Lerp(initLocation, endLocation, prog);
            transform.localRotation = Quaternion.Lerp(initRotation, Quaternion.LookRotation(endLocation - initLocation), prog * 2);
        }
        if (enemy)
        {
            StatManager enemyManager = enemy.GetComponent<StatManager>();
            enemyManager.DealDamage(Manager.GetStat(NumericalStats.PrimaryDamage));
        }
        while (prog > 0)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            if (enemy && enemy.GetComponent<StatManager>().GetHealth() > 0) endLocation = transform.InverseTransformPoint(enemy.transform.position);
            speed = Vector3.Distance(initLocation, endLocation) != 0 ? Time.deltaTime * (travelSpeed / Vector3.Distance(initLocation, endLocation)) : Time.deltaTime * travelSpeed;
            prog -= speed;
            transform.localPosition = Vector3.Lerp(initLocation, endLocation, prog);
            transform.localRotation = Quaternion.Lerp(initRotation, Quaternion.LookRotation(endLocation - initLocation), prog * 2);
        }
        attackInProg = false;
    }

    private void AmbientMovementHandler()
    {
        //TODO
    }
    
}
