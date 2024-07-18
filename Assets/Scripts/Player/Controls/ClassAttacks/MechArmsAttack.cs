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
        if (isServer && arms.Count < 8)
        {
            double currAttackSpeed = statManager.GetStat(NumericalStats.AttackSpeed);
            if (currAttackSpeed - lastAttackSpeed >= baseAttackSpeed * armScaleFactor)
            {
                lastAttackSpeed = currAttackSpeed;
                AddArm();
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
        arm.GetComponent<ArmManager>().Init(gameObject);
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
    protected override void OnSecondaryAttack()
    {
        //1 Arm for Grappling hook, allowing swinging
        if (isServer)
        {
            ArmManager arm = GetFreeArm();
            if (arm != null)
            {
                //Reset
                arm.ToggleActive(true);
            }
        }
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
        //Toggle for LMB Ability
    }

    //R
    protected override void OnAbility4()
    {
        //Bayblade using all remaining arms (ie. use after other abilities)
    }
}
