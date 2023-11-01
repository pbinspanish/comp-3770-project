using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerAnimate : MonoBehaviour
{
    [Header("References")]
    public PlayerMove movement;
    private Animator playerAnimator;

    //Animation Movement Direction
    private Vector3 animationDirection;

    // Start is called before the first frame update
    void Start()
    {
        playerAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        animationDirection = movement.movementDirection.normalized;
        animationDirection = transform.InverseTransformDirection(animationDirection);

        float vertical = Mathf.Round(animationDirection.z);
        float horizontal = Mathf.Round(animationDirection.x);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            vertical *= 2f;
            horizontal *= 2f;
        }

        playerAnimator.SetFloat("Vertical", vertical, 0.05f, Time.deltaTime);
        playerAnimator.SetFloat("Horizontal", horizontal, 0.05f, Time.deltaTime);
        playerAnimator.SetBool("isGrounded", movement.isGrounded);
    }
}
