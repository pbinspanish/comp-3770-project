using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static Unity.Burst.Intrinsics.Arm;

public class DemonAnimate : MonoBehaviour
{
    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        GameObject.FindObjectOfType(typeof(DemonScript)).GetComponent<NavMeshAgent>().enabled = false;
        GetComponent<DialogueInteractable>().ContinueDialogue();
        player.GetComponent<DialogueInitiator>().currentDialogueInteractable = GetComponent<DialogueInteractable>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            player = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Destroy(gameObject);
    }
}
