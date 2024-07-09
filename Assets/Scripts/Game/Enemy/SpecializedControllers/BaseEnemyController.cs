using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnemyController : EnemyController
{
    protected override void Start()
    {
        base.Start();
    }

    protected override void AttackFunctionality(GameObject Player, Vector3 dir)
    {
        base.AttackFunctionality(Player, dir);
        if (ValidatePlayer(Player))
        {
            GameObject proj = Instantiate(EnemyProjPrefab);
            Vector3 ProjLocation = ProjectileOffset != null ? ProjectileOffset.transform.position : transform.position;
            proj.GetComponent<ProjectileCreator>().InitializeProjectile(gameObject, ProjLocation, dir, Manager.GetStat(NumericalStats.PrimaryDamage), true);
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
        Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, BaseRotationSpeed * Time.deltaTime);
    }
}
