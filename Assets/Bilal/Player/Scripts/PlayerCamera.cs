using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    //Rotation
    private float smoothTime = 0.0f;
    private float rotationSpeed = 90f;
    private Vector3 offset;
    private Vector3 smoothVelocity;

    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - player.position;
    }

    void FixedUpdate()
    {
        //camera smoothly follows player
        transform.position = Vector3.SmoothDamp(transform.position, player.position + offset, ref smoothVelocity, smoothTime);

        //rotate camera around player with Q & E keys
        if (Input.GetKey(KeyCode.Q))
        {
            transform.RotateAround(player.position, Vector3.up, rotationSpeed * Time.fixedDeltaTime);
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.RotateAround(player.position, Vector3.up, -rotationSpeed * Time.fixedDeltaTime);
        }
    }
}
