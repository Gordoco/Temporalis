using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrafeEnemyController : EnemyController
{
    [SerializeField] private GameObject RotatingComponent;
    [SerializeField] private float DirectionSwapTimer = 6f;
    [SerializeField] private float RotationSpeed = 10f;

    private float travelDirection = 1;
    private float count = 0;

    protected override void Start()
    {
        base.Start();
        travelDirection = Random.Range(0, 1) == 0 ? 1 : -1;
    }

    protected override void AttackFunctionality(GameObject Player, Vector3 dir)
    {
        //If has RotatingComponent only allow firing if within acceptable range (0.5 degrees)
        if (RotatingComponent != null)
        {
            Quaternion lookAtRotation = Quaternion.LookRotation(Player.transform.position - RotatingComponent.transform.position);
            float angle = 1 - Mathf.Abs(Quaternion.Dot(RotatingComponent.transform.rotation, lookAtRotation));
            if (angle > 0.00872665) return;
        }

        if (bCanAttack && ValidatePlayer(Player))
        {
            base.AttackFunctionality(Player, dir);
            GameObject proj = Instantiate(EnemyProjPrefab);
            Vector3 ProjLocation = ProjectileOffset != null ? ProjectileOffset.transform.position : transform.position;
            proj.GetComponent<ProjectileCreator>().InitializeProjectile(gameObject, ProjLocation, (Player.transform.position - ProjLocation).normalized, Manager.GetStat(NumericalStats.PrimaryDamage), true);
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
    protected override void InRangeBehavior(GameObject Player, ref Vector3 destination)
    {
        Vector3 LookDir = Player.transform.position - transform.position;

        Vector3 dir = Vector3.Cross(LookDir, Vector3.up) * travelDirection;
        destination = dir;
        dir.Normalize();
        destination += transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, BaseRotationSpeed * Time.deltaTime);
        
        count+=Time.deltaTime;

        if (count > DirectionSwapTimer)
        {
            if (Random.Range(0, 6) == 0)
            {
                travelDirection *= -1;
            }
            count = 0;
        }

        //Seperates "Turret" from "Body"
        if (RotatingComponent != null)
        {
            Quaternion lookAtRotation = Quaternion.LookRotation(Player.transform.position - RotatingComponent.transform.position);
            RotatingComponent.transform.rotation = Quaternion.RotateTowards(RotatingComponent.transform.rotation, lookAtRotation, RotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Simply move towards player position as usual, rotate body to direction of movement
    /// </summary>
    /// <param name="Player"></param>
    /// <param name="dir"></param>
    /// <exception cref="System.NotImplementedException"></exception>
    protected override void OutOfRangeBehavior(GameObject Player, ref Vector3 destination)
    {
        Vector3 LookDir = Player.transform.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, BaseRotationSpeed * Time.deltaTime);

        //Seperates "Turret" from "Body"
        if (RotatingComponent != null)
        {
            Quaternion turretTargetRotation = Quaternion.LookRotation(agent.velocity.normalized);
            RotatingComponent.transform.rotation = Quaternion.RotateTowards(RotatingComponent.transform.rotation, turretTargetRotation, RotationSpeed * Time.deltaTime);
        }
    }

    protected override void VisualAttackCue()
    {
        //throw new System.NotImplementedException();
    }
}
