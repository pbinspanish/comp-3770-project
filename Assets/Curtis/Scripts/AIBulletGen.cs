using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBulletGen : MonoBehaviour
{
    // Start is called before the first frame update
    //this script will be used to just shoot bullets, nothing else
    //ok i'm a big dumb dumb, for rotating bullets, i can just handle it in here, since i could just use quaternions and whatnot to handle it in fire
    public float fireRate;
    public ProjectileLauncher launcher;
    public float rotationSpeed = 1.0f;//angle per second, if you don't want to rotate, just set to 0
    //public bool rotating = false;
    public bool reverse = false;//not rotation reverse, think bullet instantiates from outside that goes into middle
    public float radius = 10.0f;//basically how far the bullets instantiate from the middle
    public float delay = 0;
    //WEE WOO WEE WOO DON'T HANDLE THE ABOVE TWO HERE WEE WOO WEE WOO
    //public int col;
    //public List<Vector3> orgRotation = new List<Vector3>();
    private Vector3 orgRotation;
    private bool summoned=false;
    void Start()
    {
        orgRotation = Vector3.forward;
        //InvokeRepeating("fire", 0, fireRate);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameObject.FindGameObjectWithTag("Player") != null && !summoned)
        {
            InvokeRepeating("fire", delay, fireRate);
            summoned = true;
        }
        if (!reverse)
        {
            orgRotation = Quaternion.Euler(0, rotationSpeed, 0) * orgRotation;
        }
        else
        {
            orgRotation = Quaternion.Euler(0, rotationSpeed, 0) * orgRotation;
            orgRotation = Quaternion.Euler(0, 180, 0) * orgRotation;
        }
        if (GetComponent<HP>().health <= 0)
        {
            CancelInvoke();
        }
    }
    void fire()
    {
        if (!reverse)
        {
            launcher.FireProjectile_AI(transform.position, orgRotation, CharaTeam.enemy);
        }
        else
        {
            launcher.FireProjectile_AI(transform.position + radius * orgRotation, orgRotation, CharaTeam.enemy);//DO NOT FUCKING HANDLE REVERSE IN HERE, IT BRICKS EVERYTHING
        }


    }
}
