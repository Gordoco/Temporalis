using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunEnemyController : EnemyController
{
    protected override void AttackFunctionality(GameObject Player, Vector3 dir)
    {
        if (bCanAttack && ValidatePlayer(Player))
        {
            base.AttackFunctionality(Player, dir);
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

    protected override void InRangeBehavior(GameObject Player, ref Vector3 destination)
    {
        Vector3 LookDir = Player.transform.position - transform.position;
        destination = transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(LookDir.x, 0, LookDir.z));
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, BaseRotationSpeed * Time.deltaTime);
    }

    protected override void OutOfRangeBehavior(GameObject Player, ref Vector3 destination)
    {
        Vector3 LookDir = Player.transform.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, BaseRotationSpeed * Time.deltaTime);
    }
}
