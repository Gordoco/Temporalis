using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunEnemyController : EnemyController
{
    protected override void AttackFunctionality(GameObject[] Players, Vector3 dir)
    {
        if (dir.magnitude <= Manager.GetStat(NumericalStats.Range) && bCanAttack)
        {
            if (playerTarget >= 0 && playerTarget < Players.Length)
            {
                bCanAttack = false;
                StartCoroutine(AttackCooldown());
                Players[playerTarget].GetComponent<HitManager>().Stun((float)Manager.GetStat(NumericalStats.PrimaryDamage));
            }
        }
    }
}
