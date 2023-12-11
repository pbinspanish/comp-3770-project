using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class PlayerAnimate : MonoBehaviour
{
    [Header("References")]
    private float speed;
    public float walkSpeed = 1f;
    public float runSpeed = 1.3f;
    public float enemySpeed = 1f;
    private Animator playerAnimator;

    //Animation Movement Direction
    private Vector3 animationDirection;

    public bool punch;

    // Start is called before the first frame update
    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        playerAnimator.SetBool("isGrounded", true);
        playerAnimator.SetBool("Sleep", true);
    }

    // Update is called once per frame
    void Update()
    {
        if (CompareTag("Player"))
        {
            if (GetComponent<DialogueInitiator>().isInConversation || GetComponent<PlayerMove>().zombie)
            {
                animationDirection = Vector3.zero;
                playerAnimator.SetFloat("Vertical", 0f, 0.05f, Time.deltaTime);
                playerAnimator.SetFloat("Horizontal", 0f, 0.05f, Time.deltaTime);
                playerAnimator.SetBool("isGrounded", true);
            }
            else
            {
                animationDirection = GetComponent<PlayerMove>().movementDirection.normalized;
                animationDirection = transform.InverseTransformDirection(animationDirection);

                float vertical = Mathf.Round(animationDirection.z);
                float horizontal = Mathf.Round(animationDirection.x);

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    vertical *= 2f;
                    horizontal *= 2f;
                    speed = runSpeed;
                }
                else
                {
                    speed = walkSpeed;
                }

                playerAnimator.speed = speed;
                playerAnimator.SetFloat("Vertical", vertical, 0.05f, Time.deltaTime);
                playerAnimator.SetFloat("Horizontal", horizontal, 0.05f, Time.deltaTime);
                playerAnimator.SetBool("isGrounded", GetComponent<PlayerMove>().isGrounded);
            }
        }
        else
        {
            if (GetComponent<NavMeshAgent>().velocity.magnitude > 0)
            {
                playerAnimator.speed = enemySpeed;
                playerAnimator.SetFloat("Vertical", 1f, 0.05f, Time.deltaTime);
                playerAnimator.SetFloat("Horizontal", 0f, 0.05f, Time.deltaTime);
            }
            else
            {
                playerAnimator.SetFloat("Vertical", 0f, 0.05f, Time.deltaTime);
            }
            playerAnimator.SetBool("isGrounded", true);
            Debug.Log("Enemy: " + GetComponent<NavMeshAgent>().velocity.magnitude);
        }

        if (GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Punch") && GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            punch = false;
            GetComponent<Animator>().SetBool("Punch", punch);
        }
        if (GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("SwordAttack") && GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            punch = false;
            GetComponent<Animator>().SetBool("Attack", punch);
        }
    }

    public void meleePunch(Collider target, float attackDamage)
    {
        Debug.Log(target.gameObject.name);
        if (GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("WalkBS") && !punch)
        {
            punch = true;
            if (GetComponent<PlayerMove>().hasSword)
            {
                GetComponent<Animator>().SetBool("Attack", true);
            }
            else
            {
                GetComponent<Animator>().SetBool("Punch", true);
            }
            
            target.GetComponent<HP>().DealDamage(attackDamage);
        }
    }
}
