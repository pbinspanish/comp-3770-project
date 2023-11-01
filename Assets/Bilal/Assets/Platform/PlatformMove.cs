using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlatformMove : MonoBehaviour
{
    public float offset = 3f; //distance to move
    private float speed = 0f; //speed of movement
    public float rate = 1f; //rate of movement
    private bool movePositive = true; //move in the position direction?

    // Update is called once per frame
    void FixedUpdate()
    {
        //if moving in the positive direction, increase speed by offset*deltaTime
        //else moving in the negative direction, decrease speed by offset*deltaTime
        if (movePositive)
        {
            speed = (offset * Time.fixedDeltaTime);
        }
        else
        {
            speed = -(offset * Time.fixedDeltaTime);
        }

        //move the object on the x axis
        gameObject.transform.Translate(speed * rate, 0, 0);
      

        //store object's current position on the x axis
        var position = gameObject.transform.position.x;

        //if object reaches its defined offset in either direction, move in the opposite direction
        if (position >= offset)
        {
            movePositive = false;
        }
        if (position <= -offset)
        {
            movePositive = true;
        }
    }
}
