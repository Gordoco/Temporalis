using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;

[RequireComponent(typeof(StatManager))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(SoundManager))]
public abstract class EnemyController : NetworkBehaviour
{
    [SerializeField] protected GameObject EnemyProjPrefab;
    [SerializeField] private float gravity = 20;
    [SerializeField] protected float BaseRotationSpeed = 20f;
    [SerializeField] protected GameObject ProjectileOffset = null;
    [SerializeField] private AudioClip ShotSound;

    private GameObject Player;

    protected StatManager Manager;
    protected CharacterController controller;
    protected NavMeshAgent agent;

    protected bool bCanAttack = true;

    protected int AnimMovingHash;
    protected Animator animator;

    protected const float OVER_RANGE_APPROX = 1.25f;

    private bool bInRange = false;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        AudioCollection.RegisterAudioClip(ShotSound);
        if (!isServer) return;
        Manager = GetComponent<StatManager>();
        controller = GetComponent<CharacterController>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = (float)Manager.GetStat(NumericalStats.MovementSpeed);
        agent.stoppingDistance = 0.5f;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        AnimMovingHash = Animator.StringToHash("Moving");
        animator = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// Returns a random player currently connected from the level
    /// </summary>
    /// <returns></returns>
    [Server]
    protected GameObject GetRandomPlayer()
    {
        GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");
        if (Players.Length == 0) return null;
        int playerTarget = Random.Range(0, Players.Length);
        return Players[playerTarget];
    }

    /// <summary>
    /// Validates the supplied GameObject as an alive and valid player
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    [Server]
    protected bool ValidatePlayer(GameObject player)
    {
        if (player == null) return false;
        if (player.tag != "Player") return false;
        if (!player.GetComponent<PlayerStatManager>()) return false;
        if (player.GetComponent<PlayerStatManager>().GetStat(NumericalStats.Health) <= 0) return false;
        return true;
    }

    /// <summary>
    /// Core Logic loop for NPC Agents
    /// </summary>
    void Update()
    {
        if (!isServer) return;
        if (!controller.enabled) return;

        if (!Player) Player = GetRandomPlayer();
        if (!ValidatePlayer(Player)) return;

        Vector3 dir = ProjectileOffset != null ? Player.transform.position - ProjectileOffset.transform.position : Player.transform.position - transform.position;
        Vector3 destination = Player.transform.position;

        float dist = Vector3.Distance(Player.transform.position, transform.position);

        if (dist > Manager.GetStat(NumericalStats.Range))
        {
            bInRange = false;
        }
        else if (dist <= Manager.GetStat(NumericalStats.Range) / OVER_RANGE_APPROX || bInRange)
        {
            InRangeBehavior(Player, ref destination);
            if (bCanAttack)
            {
                bCanAttack = false;
                AttackFunctionality(Player, dir);
                VisualAttackCue();
                AudioAttackCue();
            }
            bInRange = true;
        }

        if (!bInRange)
        {
            OutOfRangeBehavior(Player, ref destination);
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
        //Move((float)Manager.GetStat(NumericalStats.MovementSpeed) * Time.deltaTime * dir);
        Move(destination);

        if (!animator) return;
        if (agent.velocity.magnitude != 0)
        {
            animator.SetBool(AnimMovingHash, true);
        }
        else
        {
            animator.SetBool(AnimMovingHash, false);
        }
    }

    [Server]
    protected void Move(Vector3 location)
    {
        RaycastHit hit;
        Physics.Raycast(new Vector3(location.x, 99, location.z), Vector3.down, out hit, int.MaxValue);
        agent.destination = hit.point;
        agent.velocity = agent.desiredVelocity; //Instant movement changing
    }

    /// <summary>
    /// Simple IEnumerator that utilizes the Stat Manager to enforce attack speed cooldown
    /// </summary>
    /// <returns></returns>
    [Server]
    protected IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(1 / (float)Manager.GetStat(NumericalStats.AttackSpeed));
        bCanAttack = true;
    }

    /// <summary>
    /// Functional Implementation of attack logic. Must check bCanAttack and validate Player parameter
    /// </summary>
    /// <param name="Player"></param>
    /// <param name="dir"></param>
    [Server]
    protected virtual void AttackFunctionality(GameObject Player, Vector3 dir)
    {
        StartCoroutine(AttackCooldown());
    }

    /// <summary>
    /// Implementation of the visual cue for the enemy's attack
    /// </summary>
    [Server]
    protected virtual void VisualAttackCue() { }

    /// <summary>
    /// Implementation of the audio cue for the enemy's attack
    /// </summary>
    [Server]
    protected virtual void AudioAttackCue()
    {
        if (ShotSound) GetComponent<SoundManager>().PlaySoundEffect(ShotSound);
    }

    /// <summary>
    /// Behavior agent utilizes when in range of the targeted player
    /// </summary>
    /// <param name="Player"></param>
    /// <param name="dir"></param>
    protected abstract void InRangeBehavior(GameObject Player, ref Vector3 destination);

    /// <summary>
    /// Behavior agent utilizes when not in range of the targeted player
    /// </summary>
    /// <param name="Player"></param>
    /// <param name="dir"></param>
    protected abstract void OutOfRangeBehavior(GameObject Player, ref Vector3 destination);
}
