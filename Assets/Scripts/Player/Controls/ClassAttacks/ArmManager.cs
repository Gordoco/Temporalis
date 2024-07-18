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
        initLocation = transform.position - owner.transform.position;
        initRotation = transform.rotation;
        Owner = owner;
        Manager = owner.GetComponent<PlayerStatManager>();
        travelSpeed = (float)Manager.GetStat(NumericalStats.AttackSpeed) * 25;
        if (!Manager) Debug.LogError("ERROR - [ArmManager.cs - Attempted to initialize an arm on non-player]");
        ToggleActive(true);
    }

    private void Start()
    {
        if (!isServer) return;
        RandomizeDir();
        RandomizeSpeed();
    }

    public Vector3 GetInitLocation()
    {
        return RotatePointAroundPivot(initLocation + Owner.transform.position, Owner.transform.position, Owner.transform.rotation.eulerAngles);
    }

    private void RandomizeDir()
    {
        dir.x = Random.Range(0, 2) == 0 ? 1 : -1;
        dir.y = Random.Range(0, 2) == 0 ? 1 : -1;
        dir.z = Random.Range(0, 2) == 0 ? 1 : -1;
    }

    private void RandomizeSpeed()
    {
        speed.x = Random.Range(0.1f, 0.4f);
        speed.y = Random.Range(0.1f, 0.4f);
        speed.z = Random.Range(0.1f, 0.4f);
    }

    [Server]
    public void ToggleActive(bool newActive)
    {
        bActive = newActive;
        ResetActive();
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
            if (!attackInProg) AmbientMovementHandler();
        }
    }

    private void ResetActive()
    {
        StopAllCoroutines();
        transform.position = GetInitLocation();
    }

    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(4 / (float) Manager.GetStat(NumericalStats.AttackSpeed));
        bCanAttack = true;
    }

    private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) {
          Vector3 dir = point - pivot; // get point direction relative to pivot
          dir = Quaternion.Euler(angles) * dir; // rotate it
          point = dir + pivot; // calculate rotated point
          return point; // return it
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
        Vector3 endLocation = enemy ? enemy.transform.position : GetInitLocation();
        float speed = Time.deltaTime * (travelSpeed / Vector3.Distance(GetInitLocation(), endLocation));
        while (prog < 1)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            if (enemy && enemy.GetComponent<StatManager>().GetHealth() > 0) endLocation = enemy.transform.position;
            speed = Vector3.Distance(GetInitLocation(), endLocation) != 0 ? Time.deltaTime * (travelSpeed / Vector3.Distance(GetInitLocation(), endLocation)) : Time.deltaTime * travelSpeed;
            prog += speed;
            transform.position = Vector3.Lerp(GetInitLocation(), endLocation, prog);
            transform.rotation = Quaternion.Lerp(initRotation, Quaternion.LookRotation(endLocation - GetInitLocation()), prog * 10);
        }
        if (enemy)
        {
            StatManager enemyManager = enemy.GetComponent<StatManager>();
            enemyManager.DealDamage(Manager.GetStat(NumericalStats.PrimaryDamage));
        }
        while (prog > 0)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            if (enemy && enemy.GetComponent<StatManager>().GetHealth() > 0) endLocation = enemy.transform.position;
            speed = Vector3.Distance(GetInitLocation(), endLocation) != 0 ? Time.deltaTime * (travelSpeed / Vector3.Distance(GetInitLocation(), endLocation)) : Time.deltaTime * travelSpeed;
            prog -= speed;
            transform.position = Vector3.Lerp(GetInitLocation(), endLocation, prog);
            transform.rotation = Quaternion.Lerp(Owner.transform.rotation, Quaternion.LookRotation(endLocation - GetInitLocation()), prog * 10);
        }
        attackInProg = false;
    }

    float prog = 0;
    Vector3 dir = Vector3.one;
    Vector3 speed = Vector3.one;
    bool bReversed = false;
    int progDir = 1;
    private void AmbientMovementHandler()
    {
        Vector3 modifiedInitLocation = GetInitLocation();
        float x = LerpVal(modifiedInitLocation.x, dir.x, speed.x, prog);
        float y = LerpVal(modifiedInitLocation.y, dir.y, speed.y, prog);
        float z = LerpVal(modifiedInitLocation.z, dir.z, speed.z, prog);
        prog += Time.deltaTime * 0.3f * progDir;
        transform.position = new Vector3(x, y, z);
        transform.rotation = Owner.transform.rotation;
        if (prog > 1 || prog < 0)
        {
            if (!bReversed)
            {
                progDir = -1;
                prog = 1;
            }
            else
            {
                RandomizeSpeed();
                RandomizeDir();
                progDir = 1;
                prog = 0;
            }
            bReversed = !bReversed;
        }
    }

    private float LerpVal(float init, float dir, float speed, float prog)
    {
        return Mathf.Lerp(init, init + (speed * dir), prog);
    }
}
