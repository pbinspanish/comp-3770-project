using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraMovement : MonoBehaviour
{
    //reference to use player transform
    public Transform player;
    //offset between camera and player
    public Vector3 offset;
    //Smooth camera follow
    public float smoothTime = 0f;
    //empty vector to use in SmoothDamp call
    private Vector3 smoothVelocity = Vector3.zero;

    int cameraAngle = 0;
    bool changeMode = false;
    public Camera playerCamera;

    // Start is called before the first frame update
    void Start()
    {
        //calculate offset between camera and player
        offset = transform.position - player.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //calculate camera target position
        Vector3 targetPosition = player.position + offset;
        //camera follows player from its original position to the target position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref smoothVelocity, smoothTime);

        //rotate camera around player
        //transform.RotateAround(player.position, Vector3.up, Input.GetAxis("Mouse X") * 2);

        if (Input.GetKey(KeyCode.Tab) && !changeMode)
        {
            switch (cameraAngle)
            {
                case 0:
                    cameraAngle = 1;
                    break;
                case 1:
                    cameraAngle = 0;
                    break;
            }
        }

        //transform.RotateAround(player.position, transform.right, Input.GetAxis("Mouse Y") * -2);
    }
}
