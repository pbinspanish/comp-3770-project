using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class PlayerAnimate : MonoBehaviour
{
    [Header("References")]
    public PlayerMove movement;
    private float speed;
    public float walkSpeed = 1f;
    public float runSpeed = 1.3f;
    private Animator playerAnimator;

    //Animation Movement Direction
    private Vector3 animationDirection;

    private bool punch;

    // Start is called before the first frame update
    void Start()
    {
        playerAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (CompareTag("Player"))
        {
            if (GetComponent<DialogueInitiator>().isInConversation)
            {
                animationDirection = Vector3.zero;
            }
            else
            {
                animationDirection = movement.movementDirection.normalized;
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
                playerAnimator.SetBool("isGrounded", movement.isGrounded);
            }
        }
        else
        {
            if (GetComponent<NavMeshAgent>().velocity.magnitude > 0)
            {
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
    }

    public void meleePunch(Collider target, float attackDamage)
    {
        if (GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("WalkBS") && !punch)
        {
            punch = true;
            GetComponent<Animator>().SetBool("Punch", punch);
        }

        target.GetComponent<HP>().DealDamage(attackDamage);
    }
}
