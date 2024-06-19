using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrafeEnemyController : EnemyController
{
    [SerializeField] private GameObject RotatingComponent;
    [SerializeField] private float DirectionSwapTimer = 6f;

    private float travelDirection = 1;
    private float count = 0;

    protected override void Start()
    {
        base.Start();
        travelDirection = Random.Range(0, 1) == 0 ? 1 : -1;
    }

    protected override void AttackFunctionality(GameObject Player, ref Vector3 dir)
    {
        if (bCanAttack && ValidatePlayer(Player))
        {
            base.AttackFunctionality(Player, ref dir);
            GameObject proj = Instantiate(EnemyProjPrefab);
            Vector3 ProjLocation = ProjectileOffset != null ? ProjectileOffset.transform.position : transform.position;
            proj.GetComponent<ProjectileCreator>().InitializeProjectile(gameObject, ProjLocation, dir, Manager.GetStat(NumericalStats.PrimaryDamage), true);
            //Seperates "Turret" from "Body"
            if (RotatingComponent != null)
            {
                Quaternion lookAtRotation = Quaternion.LookRotation(dir);
                RotatingComponent.transform.localRotation = lookAtRotation;
            }
        }
    }

    protected override void AudioAttackCue()
    {
        //throw new System.NotImplementedException();
    }

    /// <summary>
    /// Continuously circle player near attack range, alternating direction at set intervals
    /// </summary>
    /// <param name="Player"></param>
    /// <param name="dir"></param>
    /// <exception cref="System.NotImplementedException"></exception>
    protected override void InRangeBehavior(GameObject Player, ref Vector3 dir)
    {
        Vector3 LookDir = Player.transform.position - transform.position;

        dir = Vector3.Cross(LookDir, Vector3.up) * travelDirection;
        transform.rotation = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
        
        count+=Time.deltaTime;

        if (count > DirectionSwapTimer)
        {
            if (Random.Range(0, 6) == 0)
            {
                travelDirection *= -1;
            }
            count = 0;
        }
    }

    /// <summary>
    /// Simply move towards player position as usual, rotate body to direction of movement
    /// </summary>
    /// <param name="Player"></param>
    /// <param name="dir"></param>
    /// <exception cref="System.NotImplementedException"></exception>
    protected override void OutOfRangeBehavior(GameObject Player, ref Vector3 dir)
    {
        Vector3 LookDir = Player.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(new Vector3(LookDir.x, 0, LookDir.z));
    }

    protected override void VisualAttackCue()
    {
        //throw new System.NotImplementedException();
    }
}
