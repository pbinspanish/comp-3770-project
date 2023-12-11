using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimator : MonoBehaviour
{

    private Animator playerAnimator;
    public float enemySpeed = 1f;
    public bool punch;
// Start is called before the first frame update
void Start()
    {
        playerAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<NavMeshAgent>().velocity.magnitude > 0)
        {
            playerAnimator.speed = enemySpeed;
            playerAnimator.SetBool("Walk", true);
        }
        else
        {
            playerAnimator.SetBool("Walk", false);
        }
        Debug.Log("Enemy: " + GetComponent<NavMeshAgent>().velocity.magnitude);

        if (GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Punch") && GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            punch = false;
            GetComponent<Animator>().SetBool("Punch", punch);
        }
    }

    public void meleePunch(Collider target, float attackDamage)
    {
        Debug.Log(target.gameObject.name);
        if (GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("WalkForward") && !punch)
        {
            punch = true;
            

            GetComponent<Animator>().SetBool("Punch", true);
            

            target.GetComponent<HP>().DealDamage(attackDamage);
        }
    }
}
