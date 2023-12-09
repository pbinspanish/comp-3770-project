using UnityEngine;
using System.Collections;
public class MeleeAttack : MonoBehaviour
{
    public float damage = 1f;
    public float attackRange = 1.5f;
    public LayerMask enemyLayer;

    void Start()
    {
        // Set the enemyLayer variable to the Enemy layer
        enemyLayer = LayerMask.GetMask("Enemy");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            PerformMeleeAttack();
        }
    }

    void PerformMeleeAttack()
    {
        //Debug.Log("i'm Attacking");
        Vector3 attackPosition = transform.position + transform.forward * attackRange;
        Collider[] hitEnemies = Physics.OverlapSphere(attackPosition, attackRange, enemyLayer);
        foreach (Collider enemy in hitEnemies)
        {
            //enemy.GetComponent<HP>().DealDamage(damage);
            GetComponent<PlayerAnimate>().meleePunch(enemy, damage);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        // Calculate the attack position here as well
        Vector3 attackPosition = transform.position + transform.forward * attackRange;
        Gizmos.DrawWireSphere(attackPosition, attackRange);
    }
}

