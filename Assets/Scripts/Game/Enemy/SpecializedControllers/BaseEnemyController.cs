using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnemyController : EnemyController
{
    protected override void AttackFunctionality(GameObject Player, ref Vector3 dir)
    {
        if (bCanAttack && ValidatePlayer(Player))
        {
            base.AttackFunctionality(Player, ref dir);
            GameObject proj = Instantiate(EnemyProjPrefab);
            Vector3 ProjLocation = ProjectileOffset != null ? ProjectileOffset.transform.position : transform.position;
            proj.GetComponent<ProjectileCreator>().InitializeProjectile(gameObject, ProjLocation, dir, Manager.GetStat(NumericalStats.PrimaryDamage), true);
        }
    }

    protected override void VisualAttackCue()
    {

    }

    protected override void AudioAttackCue()
    {

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
