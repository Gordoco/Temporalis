using System.Collections;
using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class MechArmsAttack : AttackManager
{
    [SerializeField] GameObject[] ArmSpawnLocations;
    [SerializeField] GameObject ArmPrefab;

    /// <summary>
    /// Represents percent that attack speed has to increase to manifest another arm
    /// </summary>
    [SerializeField] float armScaleFactor = 0.25f;

    [SyncVar] List<GameObject> arms = new List<GameObject>(); //Synced for reference use by clients

    private double lastAttackSpeed = -1;
    private double baseAttackSpeed = -1;

    private bool bSwinging = false;

    protected override void Start()
    {
        base.Start();
        if (isServer)
        {
            lastAttackSpeed = statManager.GetStat(NumericalStats.AttackSpeed);
            baseAttackSpeed = lastAttackSpeed;
            //Start With 1 Arm
            for (int i = 0; i < 1/*ArmSpawnLocations.Length*/; i++)
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
        base.Update();
    }

    /// <summary>
    /// Creates another of 8 possible arms for the character and allocates a spot for them in the arm array and in local player space
    /// </summary>
    private void AddArm()
    {
        if (arms.Count >= 8) return;
        GameObject arm = Instantiate(ArmPrefab);
        Debug.Log(arms.Count);
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

    }

    //RMB
    private ArmManager swingArm = null;
    protected override void OnSecondaryAttack()
    {
        //Primary Toggle
    }

    //Q
    protected override void OnAbility1()
    {
        //1 Arm for pull effect
    }

    //E
    protected override void OnAbility2()
    {
        //1 Arm for temp Shield
    }

    //L-CTRL
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
                    //Reset
                    //arm.ToggleActive(true);
                    GameObject Camera = null;
                    for (int i = 0; i < gameObject.transform.childCount; i++) if (gameObject.transform.GetChild(i).tag == "MainCamera") { Camera = gameObject.transform.GetChild(i).gameObject; break; }
                    RaycastHit hit;
                    Physics.Raycast(transform.position, Camera.transform.forward, out hit, int.MaxValue);
                    if (Vector3.Distance(transform.position, hit.point) > GetComponent<PlayerStatManager>().GetStat(NumericalStats.Range) * 1.5)
                    {
                        swingArm.ToggleActive(true);
                        swingArm = null;
                    }
                    else
                    {
                        swingArm.ExternalMovementObj = hit.point;
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
    protected override void OnAbility4()
    {
        //Bayblade using all remaining arms (ie. use after other abilities)
    }
}
