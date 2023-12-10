using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform player;

    public LayerMask whatIsGround, whatIsPlayer;

    //patrolling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public int attackDamage;

    //states
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    public bool gotPlayer = false;

    public HP healthObj;
    public float rageSpeed=7.0f;
    public int rageDamage = 15;

    /*private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
    }*/

    private void Start()
    {
        healthObj = gameObject.GetComponent<HP>();
    }

    private void Update()
    {
        //Debug.Log("work");
        if (!gotPlayer)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
            agent = GetComponent<NavMeshAgent>();
            gotPlayer = true;
        }
        else { 
            playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
            playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);
    
            if(!playerInSightRange && !playerInAttackRange) { Patrolling(); }
            if(playerInSightRange && !playerInAttackRange) { ChasePlayer(); }
            if (playerInSightRange && playerInAttackRange) { AttackPlayer(); }
        }

        if (healthObj.health <= healthObj.maxHealth / 2.0f)
        {
            agent.speed = rageSpeed;
            attackDamage = rageDamage;
        }
    
    }

    private void Patrolling()
    {
        if (!walkPointSet) { SearchWalkPoint(); }

        if (walkPointSet)
        {
            agent.SetDestination(walkPoint);

        }

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //reached walkpoint
        if(distanceToWalkPoint.magnitude < 1.0f)
        {
            walkPointSet = false;
        }
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);
        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if(Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
        {
            walkPointSet = true;
        }
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
        lookAtPlayer();
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);
        lookAtPlayer();

        if (!alreadyAttacked)
        {
            //melee attack (basically just deals damage to the player since we're in attack range)
            GetComponent<PlayerAnimate>().meleePunch(player.GetComponent<CapsuleCollider>(), attackDamage);


            alreadyAttacked = true;

            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    void lookAtPlayer()
    {
        var targetRotation = Quaternion.LookRotation(player.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5 * Time.deltaTime);
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }
}
