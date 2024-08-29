using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmManager : NetworkBehaviour
{
    public Vector3 ExternalMovementObj = Vector3.zero;

    [SerializeField] private float ambientRange = 1f;

    private GameObject Owner;
    private PlayerStatManager Manager;

    private bool bActive = false;
    private bool bResetting = false;
    private bool bAttacking = false;
    private bool bCanAttack = true;
    private bool bGrappled = false;

    private const float APPROXIMATE_EQUAL_DIST = 1f;

    private GameObject HomeLocation;

    public bool GetActive()
    {
        return bActive;
    }

    public bool GetGrappled()
    {
        return bGrappled;
    }

    public void CallForReset()
    {
        bResetting = true;
    }

    public Vector3 GetInitLocation()
    {
        return HomeLocation.transform.position;
    }

    public void ToggleActive(bool b)
    {
        bActive = b;
        bGrappled = false;
    }

    [Server]
    public void Init(GameObject owner, GameObject homeLoc)
    {
        bActive = true;
        HomeLocation = homeLoc;
        Owner = owner;
        Manager = owner.GetComponent<PlayerStatManager>();
        if (!Manager) Debug.LogError("ERROR - [ArmManager.cs - Attempted to initialize an arm on non-player]");
    }

    private float GetTravelSpeed()
    {
        return (float)Manager.GetStat(NumericalStats.AttackSpeed) * 10;
    }

    private void Start()
    {
        if (!isServer) return;

    }

    /// <summary>
    /// Applies a movement in the supplied direction at a consistent speed dictated by travelSpeed. Optionally adds a rotation in travel direction.
    /// </summary>
    /// <param name="direction">Vector to apply movement in, will be normalized</param>
    [Server]
    private void MoveInDirection(Vector3 direction, Vector3 goal)
    {
        transform.parent = null;
        direction.Normalize();
        Quaternion rot = Quaternion.LookRotation(direction);
        transform.rotation = rot;
        if (Vector3.Distance(transform.position, transform.position + direction * GetTravelSpeed() * Time.deltaTime) > Vector3.Distance(transform.position, goal))
            transform.position = goal;
        else
            transform.position += direction * GetTravelSpeed() * Time.deltaTime;
    }

    private void Update()
    {
        if (!isServer) return;
        if (bActive)
        {
            GameObject Enemy = CheckForEnemies();
            if (bCanAttack && Enemy != null)
            {
                MakeAttack(Enemy);
            }
            if (bAttacking)
            {
                AttackHandler();
            }
            else if (bResetting)
            {
                ResetPosition();
            }
            else
            {
                AmbientMovement();
            }
        }
        else if (ExternalMovementObj != Vector3.zero)
        {
            MoveInDirection(ExternalMovementObj - transform.position, ExternalMovementObj);
            if (transform.position == ExternalMovementObj)//Vector3.Distance(transform.position, ExternalMovementObj) < APPROXIMATE_EQUAL_DIST)
            {
                ExternalMovementObj = Vector3.zero;
                bGrappled = true;
            }
        }
    }

    private void MakeAttack(GameObject Enemy)
    {
        EnemyToAttack = Enemy;
        bCanAttack = false;
        bAttacking = true;
    }

    private GameObject EnemyToAttack = null;
    private void AttackHandler()
    {
        if (EnemyToAttack == null || EnemyToAttack.GetComponent<EnemyStatManager>() == null)
        {
            FinishedAttack();
            return;
        }

        Vector3 dir = EnemyToAttack.transform.position - transform.position;
        MoveInDirection(dir, EnemyToAttack.transform.position);
        if (transform.position == EnemyToAttack.transform.position)//Mathf.Abs(Vector3.Distance(transform.position, EnemyToAttack.transform.position)) < APPROXIMATE_EQUAL_DIST)
        {
            EnemyToAttack.GetComponent<EnemyStatManager>().DealDamage(Manager.GetStat(NumericalStats.PrimaryDamage));
            FinishedAttack();
        }
    }

    private void FinishedAttack()
    {
        bAttacking = false;
        bResetting = true;
    }

    Vector3 randomDir;
    private void AmbientMovement()
    {
        transform.parent = Owner.transform;
        if (randomDir == Vector3.zero) randomDir = new Vector3(Random.Range(-100, 100), Random.Range(-100, 100), Random.Range(-100, 100)).normalized;
        Vector3 nextPos = transform.localPosition + (randomDir * ambientRange);
        Vector3 dir = nextPos - transform.localPosition;
        transform.localPosition += 0.03f * Time.deltaTime * dir;
        if (Vector3.Distance(transform.localPosition, nextPos) < APPROXIMATE_EQUAL_DIST) randomDir = Vector3.zero;
    }

    private void ResetPosition()
    {
        Vector3 dir = GetInitLocation() - transform.position;
        MoveInDirection(dir, GetInitLocation());
        if (transform.position == GetInitLocation())//Mathf.Abs(Vector3.Distance(transform.position, GetInitLocation())) < APPROXIMATE_EQUAL_DIST)
        {
            transform.position = GetInitLocation();
            bResetting = false;
            bCanAttack = true;
            transform.parent = Owner.transform;
        }
    }

    private GameObject CheckForEnemies()
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, (float)Manager.GetStat(NumericalStats.Range), Vector3.up);
        float currDist = float.MaxValue;
        GameObject currClosest = null;
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.gameObject.tag != "Enemy") continue;
            GameObject Enemy = hits[i].collider.gameObject;
            float dist = Vector3.Distance(Enemy.transform.position, GetInitLocation());
            if (dist < Manager.GetStat(NumericalStats.Range) && dist < currDist)
            {
                currDist = dist;
                currClosest = Enemy;
            }
        }
        return currClosest;
    }

}