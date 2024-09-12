using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnemyController : EnemyController
{
    protected override void Start()
    {
        base.Start();
    }

    /// <summary>
    /// Simple projectile firing attack behavior
    /// </summary>
    /// <param name="Player"></param>
    /// <param name="dir"></param>
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
}
