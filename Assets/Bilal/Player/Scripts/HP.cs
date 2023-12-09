using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HP : MonoBehaviour
{
    public float health = 100f;
    public GameObject thisObject;


    // Start is called before the first frame update
    void Start()
    {
        thisObject = gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void damage (float strength)
    {
        Debug.Log("HI i'm damaging");
        health -= strength;

        if (health <= 0 )
        {
            die(thisObject);
        }
    }

    public void die(GameObject thisObject)
    {
        if (thisObject.CompareTag("Player"))
        {
            PlayerMove.respawn();
        }
        else
        {
            Destroy(thisObject);
        }
    }
}
