using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HP : MonoBehaviour
{
    public static float health = 100f;
    private static GameObject thisObject;


    // Start is called before the first frame update
    void Start()
    {
        thisObject = gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void damage (float strength)
    {
        health -= strength;

        if (health <= 0 )
        {
            die(thisObject);
        }
    }

    public static void die(GameObject thisObject)
    {
        if (thisObject.CompareTag("Player"))
        {
            PlayerMove.respawn();
            health = 100f;
        }
        else
        {
            Destroy(thisObject);
        }
    }
}
