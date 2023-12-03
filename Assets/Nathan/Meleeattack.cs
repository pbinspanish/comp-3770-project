using UnityEngine;
using System.Collections;
public class MeleeAttack : MonoBehaviour
{
    public float damage = 1f;
    public float attackRange = 1f;
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
        Vector3 attackPosition = transform.position + transform.forward * attackRange;
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);
        foreach (Collider enemy in hitEnemies)
        {
                Debug.Log("Hit");
                enemy.GetComponent<HPComponent>().Damage(1);
    
        }
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 attackPosition = transform.position + transform.forward * attackRange;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}