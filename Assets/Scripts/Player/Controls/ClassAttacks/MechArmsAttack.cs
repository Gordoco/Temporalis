using System.Collections;
using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class MechArmsAttack : AttackManager
{
    [SerializeField] GameObject[] ArmSpawnLocations;
    [SerializeField] GameObject ArmPrefab;

    [SyncVar] List<GameObject> arms = new List<GameObject>();
    protected override void Start()
    {
        base.Start();
        if (isServer)
        {
            //Start With 1 Arm
            for (int i = 0; i < 1/*ArmSpawnLocations.Length*/; i++)
            {
                AddArm();
            }
        }
    }

    private void AddArm()
    {
        if (arms.Count >= 8) return;
        GameObject arm = Instantiate(ArmPrefab);
        arm.transform.position = ArmSpawnLocations[arms.Count].transform.position;
        arm.transform.rotation = transform.rotation;
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
        bestArm.ToggleActive(false);
        return bestArm;
    }

    //LMB
    protected override void OnPrimaryAttack()
    {

    }

    //RMB
    protected override void OnSecondaryAttack()
    {

    }

    //Q
    protected override void OnAbility1()
    {

    }

    //E
    protected override void OnAbility2()
    {

    }

    //L-CTRL
    protected override void OnAbility3()
    {

    }

    //R
    protected override void OnAbility4()
    {

    }
}
