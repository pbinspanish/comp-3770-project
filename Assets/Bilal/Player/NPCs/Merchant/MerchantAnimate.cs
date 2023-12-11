using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MerchantAnimate : MonoBehaviour
{
    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player.GetComponent<DialogueInitiator>().isInConversation)
        {
            GameObject.FindGameObjectWithTag("Merchant").GetComponent<Animator>().SetBool("Talk", true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            player = other.gameObject;
        }
    }
}
