using System.Collections;
using Mirror;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class MechArmsAttack : AttackManager
{
    [SerializeField] GameObject[] ArmSpawnLocations;
    [SerializeField] GameObject ArmPrefab;
    [SerializeField] GameObject SpotterLocation;
    [SerializeField] GameObject Bubble;

    /// <summary>
    /// Represents percent that attack speed has to increase to manifest another arm
    /// </summary>
    [SerializeField] float armScaleFactor = 0.25f;

    [SyncVar] List<GameObject> arms = new List<GameObject>(); //Synced for reference use by clients

    private double lastAttackSpeed = -1;
    private double baseAttackSpeed = -1;

    private double primaryBoostFactor = 1;

    private bool bSwinging = false;

    [SyncVar] private GameObject EnemyFocus;

    public GameObject GetEnemyFocus()
    {
        return EnemyFocus;
    }

    protected override void Start()
    {
        base.Start();
        GetComponent<LineRenderer>().material.color = Color.green;
        if (isServer)
        {
            lastAttackSpeed = statManager.GetStat(NumericalStats.AttackSpeed);
            baseAttackSpeed = lastAttackSpeed;
            //Start With 1 Arm
            for (int i = 0; i < 2/*ArmSpawnLocations.Length*/; i++)
            {
                AddArm();
            }
        }
    }

    protected override void Update()
    {
        if (!isServer) return;
        if (arms.Count < 8)
        {
            double currAttackSpeed = statManager.GetStat(NumericalStats.AttackSpeed);
            if (currAttackSpeed - lastAttackSpeed >= baseAttackSpeed * armScaleFactor)
            {
                lastAttackSpeed = currAttackSpeed;
                AddArm();
            }
        }

        if (!Input.GetButton("Ability3"))
        {
            if (bSwinging)
            {
                swingArm.CallForReset();
                swingArm.ToggleActive(true);
                
                bSwinging = false;
                swingArm = null;
                GetComponent<PlayerMove>().Server_StopSwing();
            }
        }

        if (!Input.GetButton("PrimaryAttack"))
        {
            GetComponent<LineRenderer>().enabled = false;
        }
        base.Update();
    }

    /// <summary>
    /// Creates another of 8 possible arms for the character and allocates a spot for them in the arm array and in local player space
    /// </summary>
    private void AddArm()
    {
        if (arms.Count >= 8) return;
        GameObject arm = Instantiate(ArmPrefab);
        arm.transform.SetPositionAndRotation(ArmSpawnLocations[arms.Count].transform.position, transform.rotation);
        arm.GetComponent<ArmManager>().Init(gameObject, ArmSpawnLocations[arms.Count]);
        arms.Add(arm);
        NetworkServer.Spawn(arm);
    }

    /// <summary>
    /// Returns a free arm from the set of all active arms, prioritizes arms that are close to their resting position (ie. not travelling to attack)
    /// </summary>
    /// <returns></returns>
    [Server]
    private ArmManager GetFreeArm()
    {
        float currClosest = float.MaxValue;
        ArmManager bestArm = null;
        foreach (GameObject arm in arms)
        {
            ArmManager manager = arm.GetComponent<ArmManager>();
            if (manager.GetActive())
            {
                float dist = Vector3.Distance(arm.transform.position, manager.GetInitLocation());
                if (dist < currClosest)
                {
                    currClosest = dist;
                    bestArm = manager;
                }
            }
        }
        if (bestArm) bestArm.ToggleActive(false);
        return bestArm;
    }

    //LMB
    protected override void OnPrimaryAttack()
    {
        //Dual function, Spotter (double damage and priority target for arms) and Simple rifle
        GameObject Camera = null;
        for (int i = 0; i < gameObject.transform.childCount; i++) if (gameObject.transform.GetChild(i).tag == "MainCamera") { Camera = gameObject.transform.GetChild(i).gameObject; break; }
        RaycastHit hit;
        Physics.Raycast(Camera.transform.position, Camera.transform.forward, out hit, (float)statManager.GetStat(NumericalStats.Range), LayerMask.GetMask("Default"));

        if (hit.collider)
        {
            GetComponent<LineRenderer>().SetPosition(1, hit.point);
            if (isServer && hit.collider.gameObject.GetComponent<EnemyStatManager>())
            {
                hit.collider.gameObject.GetComponent<EnemyStatManager>().DealDamage(statManager.GetStat(NumericalStats.SecondaryDamage) * Time.deltaTime * primaryBoostFactor);
                EnemyFocus = hit.collider.gameObject;
            }
        }
        else
        {
            EnemyFocus = null;
            GetComponent<LineRenderer>().SetPosition(1, Camera.transform.position + (Camera.transform.forward * (float)statManager.GetStat(NumericalStats.Range)));
        }
        if (SpotterLocation) GetComponent<LineRenderer>().SetPosition(0, SpotterLocation.transform.position);
        GetComponent<LineRenderer>().enabled = true;
    }

    //RMB
    protected override void OnSecondaryAttack()
    {
        //Primary Toggle
        GetComponent<LineRenderer>().material.color = Color.red;
        StartCoroutine(ResetSecondaryBoost());
        if (!isServer) return;
        primaryBoostFactor = 10;
    }

    private IEnumerator ResetSecondaryBoost()
    {
        yield return new WaitForSeconds(30/(float)statManager.GetStat(NumericalStats.SecondaryCooldown));
        GetComponent<LineRenderer>().material.color = Color.green;
        if (!isServer) yield return 0;
        primaryBoostFactor = 1;
    }

    //Q
    protected override void OnAbility1()
    {
        //1 Arm for pull effect
        if (!isServer) return;

        ArmManager arm = GetFreeArm();

        GameObject Camera = null;
        for (int i = 0; i < gameObject.transform.childCount; i++) if (gameObject.transform.GetChild(i).tag == "MainCamera") { Camera = gameObject.transform.GetChild(i).gameObject; break; }
        RaycastHit hit;
        Physics.Raycast(Camera.transform.position, Camera.transform.forward, out hit, (float)statManager.GetStat(NumericalStats.Range) * 1.3f, LayerMask.GetMask("Default"));
        if (arm && hit.collider && hit.collider.gameObject.GetComponent<EnemyStatManager>())
        {
            //Send arm to enemy
            arm.ExternalMovementObj = hit.collider.gameObject;
            arm.ExternalMovementLoc = hit.collider.transform.InverseTransformPoint(hit.point);

            EnemyController controller = hit.collider.gameObject.GetComponent<EnemyController>();
            controller.TakeControlOfEnemy((transform.position - hit.point) * 1.25f);
            StartCoroutine(ReleaseEnemy(controller, arm));
        }
        else
        {
            bCanAbility1 = true;
            if (arm)
            {
                arm.CallForReset();
                arm.ToggleActive(true);
            }
            bIgnoreAbility1Cooldown = true;
            StartCoroutine(FixFailedAbility1());
        }
    }

    private IEnumerator ReleaseEnemy(EnemyController controller, ArmManager arm)
    {
        yield return new WaitForSeconds(1f);
        if (controller) controller.ReturnControlOfEnemy();
        arm.CallForReset();
        arm.ToggleActive(true);
    }

    private IEnumerator FixFailedAbility1()
    {
        yield return new WaitForEndOfFrame();
        bIgnoreAbility1Cooldown = false;
    }

    //E
    protected override void OnAbility2()
    {
        //1 Arm for temp Shield
        Bubble.SetActive(true);
        if (isServer)
        {
            statManager.AddShield(statManager.GetStat(NumericalStats.Ability2Damage), 5);
        }
        StartCoroutine(FinishAbility2());
    }

    private IEnumerator FinishAbility2()
    {
        yield return new WaitForSeconds(5);
        Bubble.SetActive(false);
    }

    //L-CTRL
    private ArmManager swingArm = null;
    protected override void OnAbility3()
    {
        //1 Arm for Grappling hook, allowing swinging
        if (isServer)
        {
            if (!bSwinging)
            {
                swingArm = GetFreeArm();
                if (swingArm != null)
                {
                    GameObject Camera = null;
                    for (int i = 0; i < gameObject.transform.childCount; i++) if (gameObject.transform.GetChild(i).tag == "MainCamera") { Camera = gameObject.transform.GetChild(i).gameObject; break; }
                    RaycastHit hit;
                    Physics.Raycast(Camera.transform.position, Camera.transform.forward, out hit, int.MaxValue, LayerMask.GetMask("Default"));
                    if (Vector3.Distance(Camera.transform.position, hit.point) > GetComponent<PlayerStatManager>().GetStat(NumericalStats.Range) * 1.5)
                    {
                        swingArm.ToggleActive(true);
                        swingArm = null;
                    }
                    else
                    {
                        if (!hit.collider) { swingArm.CallForReset(); swingArm.ToggleActive(true); return; }
                        Vector3 localHit = hit.collider.transform.root.InverseTransformPoint(hit.point);
                        Debug.DrawLine(transform.position, hit.collider.transform.root.position + localHit, Color.blue, 15);
                        swingArm.ExternalMovementLoc = localHit;
                        swingArm.ExternalMovementObj = hit.collider.transform.root.gameObject;
                        bSwinging = true;
                    }
                }
            }
            else
            {
                if (swingArm.GetGrappled())
                {
                    GetComponent<PlayerMove>().Server_Swing(swingArm.transform.position, Vector3.Distance(transform.position, swingArm.transform.position));
                }
            }
        }
    }

    //R
    List<ArmManager> armList;
    protected override void OnAbility4()
    {
        //Bayblade using all remaining arms (ie. use after other abilities)
        if (isServer)
        {
            armList = new List<ArmManager>();
            for (int i = 0; i < arms.Count; i++)
            {
                ArmManager manager = GetFreeArm();
                if (manager == null) break;
                else armList.Add(manager);
                manager.ToggleBeyblade();
            }
            Coroutine damageRoutine = StartCoroutine(Ability4DamageCheck());
            StartCoroutine(FinishAbility4(damageRoutine));
        }
    }

    private IEnumerator FinishAbility4(Coroutine DamageRoutine)
    {
        yield return new WaitForSeconds(5);
        if (DamageRoutine != null) StopCoroutine(DamageRoutine);
        foreach (ArmManager manager in armList)
        {
            manager.ToggleBeyblade();
            manager.CallForReset();
            manager.ToggleActive(true);
        }
        armList.Clear();
    }

    private IEnumerator Ability4DamageCheck()
    {
        if (!isServer) yield return 0;
        while (true)
        {
            yield return new WaitForSeconds(1 / ((float)statManager.GetStat(NumericalStats.AttackSpeed) * armList.Count));
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, Vector3.Distance(armList[0].transform.position, transform.position) + 1f, Vector3.up, 0.01f);

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider && hit.collider.gameObject.GetComponent<EnemyStatManager>())
                {
                    hit.collider.gameObject.GetComponent<EnemyStatManager>().DealDamage(statManager.GetStat(NumericalStats.Ability4Damage));
                }
            }
        }
    }
}
