using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuCamera : MonoBehaviour
{
    //distance to move
    public float distance = 3f;
    //speed of movement
    public float speed = 1;
    //move in the position direction?
    private bool movePositive = true;
    //move
    private float move;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
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
}
