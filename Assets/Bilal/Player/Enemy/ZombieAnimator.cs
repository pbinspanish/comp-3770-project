using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieAnimator : MonoBehaviour
{
    public float enemySpeed = 1f;
    public bool die = false;

    // Start is called before the first frame update
    void Start()
    {
        die = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("ZombieThriller") && GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime >= 8f)
        {
            GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        }
        if (GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("ZombieIdle"))
        {
            GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>().canMove = true;
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>().zombie = false;
            Destroy(GameObject.FindGameObjectWithTag("ZombieTrigger"));
        }

        if (GetComponent<NavMeshAgent>().velocity.magnitude > 0)
        {
            GetComponent<Animator>().speed = enemySpeed;
            GetComponent<Animator>().SetBool("Walk", true);
            GetComponent<Animator>().SetBool("Crawl", true);
        }
        else
        {
            GetComponent<Animator>().SetBool("Walk", false);
            GetComponent<Animator>().SetBool("Crawl", false);
        }
    }

    public void WakeZombie()
    {
        GetComponent<Animator>().SetBool("StandUp", true);

    }
}