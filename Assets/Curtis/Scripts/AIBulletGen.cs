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
    public Vector3 offset;
    //WEE WOO WEE WOO DON'T HANDLE THE ABOVE TWO HERE WEE WOO WEE WOO
    //public int col;
    //public List<Vector3> orgRotation = new List<Vector3>();
    private Vector3 orgRotation;
    private bool summoned=false;
    public GameObject parent;

    public int health=100;
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
            Debug.Log("boolean"+(bool)(parent.GetComponent<EnemyAI>().playerInSightRange==false));
            if ((parent.GetComponent<EnemyAI>().playerInSightRange|| parent.GetComponent<EnemyAI>().playerInAttackRange)&&parent.GetComponent<HP>().health<=health) {
                Debug.Log("Ich Nichten Lichten");
                InvokeRepeating("fire", delay, fireRate);
                summoned = true;
            }
        }
        if (parent.GetComponent<HP>().health <= 0 || parent.GetComponent<EnemyAI>().playerInSightRange == false)
        {
            Debug.Log("Hi i'm gonna stop");
            CancelInvoke();
            summoned = false;
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
    }
    void fire()
    {
        Debug.Log("AI do be firing");
        if (!reverse)
        {
            Debug.Log("Happening");
            launcher.FireProjectile_AI(gameObject.transform.position, orgRotation, CharaTeam.enemy);
        }
        else
        {
            launcher.FireProjectile_AI(gameObject.transform.position + radius * orgRotation, orgRotation, CharaTeam.enemy);//DO NOT FUCKING HANDLE REVERSE IN HERE, IT BRICKS EVERYTHING
        }


    }
}
