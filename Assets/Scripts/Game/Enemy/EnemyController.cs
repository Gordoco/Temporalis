using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(StatManager))]
[RequireComponent(typeof(CharacterController))]
public class EnemyController : NetworkBehaviour
{
    [SerializeField] protected GameObject EnemyProjPrefab;
    [SerializeField] private float gravity = 20;
    [SerializeField] protected Vector3 ProjectileOffset = Vector3.zero;

    private GameObject[] Players;

    protected StatManager Manager;
    protected CharacterController controller;

    protected bool bCanAttack = true;
    protected int playerTarget = -1;

    // Start is called before the first frame update
    void Start()
    {
        if (!isServer) return;
        Players = GameObject.FindGameObjectsWithTag("Player");
        playerTarget = Random.Range(0, Players.Length);
        Manager = GetComponent<StatManager>();
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isServer) return;
        if (!controller.enabled) return;

        if (Players == null)
        {
            Players = GameObject.FindGameObjectsWithTag("Player");
            playerTarget = Random.Range(0, Players.Length);
            return;
        }
        else if (Players.Length <= 0)
        {
            Players = GameObject.FindGameObjectsWithTag("Player");
            playerTarget = Random.Range(0, Players.Length);
            return;
        }

        if (GameObject.FindGameObjectsWithTag("Player").Length != Players.Length)
        {
            Players = GameObject.FindGameObjectsWithTag("Player");
            playerTarget = Random.Range(0, Players.Length);
            return;
        }

        if (playerTarget < 0 || playerTarget >= Players.Length)
        {
            playerTarget = Random.Range(0, Players.Length);
        }

        Vector3 dir;
        if (Players[playerTarget] != null) dir = Players[playerTarget].transform.position - (transform.position + ProjectileOffset);
        else
        {
            Players = GameObject.FindGameObjectsWithTag("Player");
            playerTarget = Random.Range(0, Players.Length);
            return;
        }

        AttackFunctionality(Players, dir);

        transform.rotation = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
        if (Vector3.Distance(Players[playerTarget].transform.position, transform.position) <= Manager.GetStat(NumericalStats.Range)/2)
        {
            dir = new Vector3(0, dir.y, 0);
            dir.Normalize();
        }
        dir.Normalize();
        
        if (controller.isGrounded)
        {
            dir.y = 0;
        }
        else
        {
            dir.y -= gravity;
        }
        controller.Move(dir * Time.deltaTime * (float)Manager.GetStat(NumericalStats.MovementSpeed));
    }

    protected IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(1/(float)Manager.GetStat(NumericalStats.AttackSpeed));
        bCanAttack = true;
    }

    protected virtual void AttackFunctionality(GameObject[] Players, Vector3 dir)
    {
        if (dir.magnitude <= Manager.GetStat(NumericalStats.Range) && bCanAttack)
        {
            bCanAttack = false;
            StartCoroutine(AttackCooldown());
            GameObject proj = Instantiate(EnemyProjPrefab);
            proj.GetComponent<ProjectileCreator>().InitializeProjectile(gameObject, transform.position + ProjectileOffset, dir, Manager.GetStat(NumericalStats.PrimaryDamage), true);
        }
    }
}
