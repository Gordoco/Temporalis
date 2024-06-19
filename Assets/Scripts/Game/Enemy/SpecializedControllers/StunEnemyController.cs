using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunEnemyController : EnemyController
{
    protected override void AttackFunctionality(GameObject Player, ref Vector3 dir)
    {
        if (bCanAttack && ValidatePlayer(Player))
        {
            base.AttackFunctionality(Player, ref dir);
            Player.GetComponent<HitManager>().Stun((float)Manager.GetStat(NumericalStats.PrimaryDamage));
        }
    }

    protected override void VisualAttackCue()
    {
        if (bCanAttack)
        {

        }
    }

    protected override void AudioAttackCue()
    {
        if (bCanAttack)
        {

        }
    }

    protected override void InRangeBehavior(GameObject Player, ref Vector3 dir)
    {
        Vector3 LookDir = Player.transform.position - transform.position;
        dir = new Vector3(0, LookDir.y, 0);
        dir.Normalize();
        transform.rotation = Quaternion.LookRotation(new Vector3(LookDir.x, 0, LookDir.z));
    }

    protected override void OutOfRangeBehavior(GameObject Player, ref Vector3 dir)
    {
        Vector3 LookDir = Player.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(new Vector3(LookDir.x, 0, LookDir.z));
    }
}
