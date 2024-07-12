using System.Collections;
using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class MechArmsAttack : AttackManager
{
    [SerializeField] GameObject[] ArmSpawnLocations;
    [SerializeField] GameObject ArmPrefab;

    protected override void Start()
    {
        base.Start();
        if (isServer)
        {
            for (int i = 0; i < ArmSpawnLocations.Length; i++)
            {
                GameObject arm = Instantiate(ArmPrefab, transform);
                arm.transform.localPosition = ArmSpawnLocations[i].transform.localPosition;
                arm.GetComponent<ArmManager>().Init(gameObject);
                NetworkServer.Spawn(arm);
            }
        }
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
