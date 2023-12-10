using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class granny : MonoBehaviour
{
    public Transform gran;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !GetComponent<AudioSource>().isPlaying)
        {
            Debug.Log("Play");
            GetComponent<AudioSource>().Play();

            gran.gameObject.GetComponent<Animator>().SetBool("Dance", true);
        }
    }
}
