using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(LineRenderer))]
public class ArmManager : NetworkBehaviour
{
    public Vector3 ExternalMovementLoc = Vector3.zero;
    public GameObject ExternalMovementObj;

    [SerializeField] private float ambientRange = 1f;

    private GameObject Owner;
    private PlayerStatManager Manager;

    private bool bActive = false;
    private bool bResetting = false;
    private bool bAttacking = false;
    private bool bCanAttack = true;
    private bool bGrappled = false;
    private bool bBeyblade = false;

    private const float APPROXIMATE_EQUAL_DIST = 1f;

    private GameObject HomeLocation;

    private float normalRadius = 0;

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

    public void ToggleBeyblade()
    {
        bBeyblade = !bBeyblade;
        ExternalMovementLoc = Vector3.zero;
        currRotation = Random.Range(0, 2 * Mathf.PI);
    }

    public void Init(GameObject owner, GameObject homeLoc)
    {
        bActive = true;
        HomeLocation = homeLoc;
        Owner = owner;
        Manager = owner.GetComponent<PlayerStatManager>();
        normalRadius = Vector3.Distance(transform.position, Owner.transform.position);
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

    private void LateUpdate()
    {
        if (Owner)
        {     
            GetComponent<LineRenderer>().SetPosition(0, transform.position);
            GetComponent<LineRenderer>().SetPosition(1, Owner.transform.position);
            //UpdateClientLineRenderer(transform.position, Owner.transform.position);
        }
    }

    [ClientRpc]
    private void UpdateClientLineRenderer(Vector3 one, Vector3 two)
    {
        GetComponent<LineRenderer>().SetPosition(0, one);
        GetComponent<LineRenderer>().SetPosition(1, two);
    }

    float currRotation = 0;
    float armCooldown = 0;
    private void Update()
    {
        //if (!isServer) return;
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
        else if (ExternalMovementLoc != Vector3.zero && ExternalMovementObj != null)
        {
            Vector3 ExternalPoint = ExternalMovementObj.transform.position + ExternalMovementLoc;
            MoveInDirection(ExternalPoint - transform.position, ExternalPoint);
            if (transform.position == ExternalPoint)//Vector3.Distance(transform.position, ExternalMovementObj) < APPROXIMATE_EQUAL_DIST)
            {
                bGrappled = true;
            }
        }
        else if (bBeyblade)
        {
            transform.parent = null;
            transform.position = GetInitLocation();
            transform.parent = Owner.transform;
            transform.localPosition = rotateAboutOwner(currRotation);
            currRotation += Time.deltaTime * (float)Manager.GetStat(NumericalStats.AttackSpeed);
        }
        else
        {
            currRotation = 0;
        }
    }

    /// <summary>
    /// Object space orbit of owner
    /// </summary>
    /// <param name="angleInRads"></param>
    /// <returns></returns>
    Vector3 rotateAboutOwner(float angleInRads)
    {
        float dist = normalRadius * 3;
        float sin = Mathf.Sin(angleInRads);
        float cos = Mathf.Cos(angleInRads);
        Vector3 newPos = new Vector3(
            dist * cos,
            0,
            dist * sin
            );
        return newPos;
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
            if (isServer) EnemyToAttack.GetComponent<EnemyStatManager>().DealDamage(Manager.GetStat(NumericalStats.PrimaryDamage));
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
        if (transform.position == GetInitLocation() && armCooldown >= (1/(float)Manager.GetStat(NumericalStats.AttackSpeed)))//Mathf.Abs(Vector3.Distance(transform.position, GetInitLocation())) < APPROXIMATE_EQUAL_DIST)
        {
            transform.position = GetInitLocation();
            bResetting = false;
            bCanAttack = true;
            transform.parent = Owner.transform;
            armCooldown = 0;
        }
        armCooldown += Time.deltaTime;
    }

    private GameObject CheckForEnemies()
    {
        MechArmsAttack attackHandler = Owner.GetComponent<MechArmsAttack>();
        if (attackHandler != null)
        {
            GameObject enemy = attackHandler.GetEnemyFocus();
            if (enemy && enemy.GetComponent<EnemyStatManager>() && enemy.GetComponent<EnemyStatManager>().GetHealth() > 0)
            {
                return enemy;
            }
        }

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