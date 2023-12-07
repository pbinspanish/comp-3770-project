using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class contains the logic for controlling the camera on the main menu screen.
/// The camera will move back and forth from the starting position to the distance
/// in the variable distance.
/// </summary>
public class MainMenuCamera : MonoBehaviour
{
    #region Variables

    public float distance = 3f;         // distance to move
    public float speed = 1;             // movement speed
    private bool movePositive = true;   // what direction to move
    private float move;                 // z-axis movement magnitude

    #endregion

    #region Methods
    
    void FixedUpdate()
    {
        //if moving in the positive direction, increase speed by offset*deltaTime
        //else moving in the negative direction, decrease speed by offset*deltaTime
        if (movePositive)
        {
            move = (distance * Time.fixedDeltaTime * speed);
            Debug.Log("forward");
        }
        else
        {
            move = -(distance * Time.fixedDeltaTime * speed);
            Debug.Log("back");
        }

        //move the object on the z axis
        gameObject.transform.Translate(0, 0, move);

        //store object's current position on the z axis
        var position = gameObject.transform.localPosition.z;
        Debug.Log(position);

        //if object reaches its defined offset in either direction, move in the opposite direction
        if (position >= distance)
        {
            movePositive = false;
            Debug.Log("above 3");
        }
        if (position <= 0f)
        {
            movePositive = true;
            Debug.Log("below 3");
        }
    }

    #endregion
}
