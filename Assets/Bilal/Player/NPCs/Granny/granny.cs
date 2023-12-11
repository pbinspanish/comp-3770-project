using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class granny : MonoBehaviour
{
    public Transform gran;
    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(transform.parent.gameObject.name);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !GetComponent<AudioSource>().isPlaying)
        {
            player = other.gameObject;

            Debug.Log("Play");
            GetComponent<AudioSource>().Play();

            gran.gameObject.GetComponent<Animator>().SetBool("Dance", true);
            player.GetComponent<Animator>().SetBool("Dance", true);
        }
    }

    public void destroy()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        player.GetComponent<Animator>().SetBool("Dance", false);
    }
}
