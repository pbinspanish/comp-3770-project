using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimator : MonoBehaviour
{

    private Animator playerAnimator;
       public float enemySpeed = 1f;
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
    }
}
