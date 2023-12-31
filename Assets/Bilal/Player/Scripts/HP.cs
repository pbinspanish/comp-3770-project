using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Contains the definition for an object with health.
/// </summary>
public class HP : MonoBehaviour
{
    private Animator animator;

    #region Variables
    public bool canDamage;
    // Data
    public float health;
    public float maxHealth = 100.0f;

    // Display
    private HUDController playerHealthHUD;
    public bool treatAsPlayer = false;
    public GameObject player;

    #endregion

    #region Methods

    private void Start()
    {
        animator = GetComponent<Animator>();
        canDamage = true;
        health = maxHealth;

        if (treatAsPlayer)
        {
            playerHealthHUD = FindObjectOfType<HUDController>();
            playerHealthHUD.minHealth = 0;
            playerHealthHUD.maxHealth = (int)maxHealth;
            playerHealthHUD.currentHealth = (int)health;
        }
    }

    private void Update()
    {
        if (treatAsPlayer)
        {
            playerHealthHUD.currentHealth = (int)health;
        }
    }
    public void ChangeDamage(){
        if(canDamage){
            canDamage = false;
        } 
        else{
            canDamage = true;
        }
    }
    /// <summary>
    /// Deals the given amount of damage to the object.
    /// </summary>
    /// <param name="amount"></param>
    public void DealDamage(float amount)
    {
        if(canDamage){
            Debug.Log("Sword " + GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>().hasSword);
            float damage = amount;
            if (name == "Wall" && GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>().hasSword && GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerAnimate>().punch)
            {
                damage = 20f;
            }
            else if(name == "Wall" && !GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>().hasSword)
            {
                return;
            }
            Debug.Log("Player: dealing " + amount + " damage");
            health -= damage;
            Debug.Log("Player: has " + health + " health");

            if (health <= 0)
            {
                Die();
            }
        }
    }

    /// <summary>
    /// Kills the attached gameobject. If the gameobject is a player, respawn. Otherwise destory the gameObject.
    /// </summary>
    private void Die()
    {
        if (CompareTag("Player"))
        {
            health = maxHealth;
            GetComponent<PlayerMove>().respawn();
        }
        else if (name == "Granny")
        {
            GetComponent<Animator>().SetBool("DieBitch", true);
            Destroy(GetComponent<Collider>());
            GetComponentInChildren<granny>().destroy();
        }
        else if (name == "Wall")
        {
            Destroy(gameObject);

            FindObjectOfType<PlayerMove>().hasBrokenVillageWall = true;
        }
        else if (GetComponent<ZombieAnimator>() != null)
        {
            if (GetComponent<ZombieAnimator>().die)
            {
                GetComponent<Animator>().SetBool("DieBitch", true);
                Destroy(GetComponent<Collider>());
                Destroy(GetComponent<NavMeshAgent>());
                GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>().zombieKill++;
            }
            else
            {
                health = maxHealth;
                GetComponent<Animator>().SetBool("Fall", true);
                GetComponent<NavMeshAgent>().enabled = false;
                GetComponent<EnemyAI>().enabled = false;
                GetComponent<ZombieAnimator>().die = true;
            }
        }
        else if(name == "Demon")
        {
            animator.SetBool("DieBitch", true);
            Destroy(GetComponent<NavMeshAgent>());
            Destroy(GetComponent<Collider>());
            Destroy(GameObject.FindGameObjectWithTag("DemonTrigger"));
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>().demonDead = true;
        }
        else
        {
            animator.SetBool("DieBitch", true);
            Destroy(GetComponent<NavMeshAgent>());
            Destroy(GetComponent<Collider>());
        }
    }

    #endregion
}
