using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunEnemyController : EnemyController
{
    protected override void Start()
    {
        base.Start();
    }

    /// <summary>
    /// Override of default attack behavior to apply stun status effect
    /// </summary>
    /// <param name="Player"></param>
    /// <param name="dir"></param>
    protected override void AttackFunctionality(GameObject Player, Vector3 dir)
    {
        base.AttackFunctionality(Player, dir);
        if (ValidatePlayer(Player))
        {
            Player.GetComponent<HitManager>().Stun((float)Manager.GetStat(NumericalStats.PrimaryDamage));
        }
    }
}
