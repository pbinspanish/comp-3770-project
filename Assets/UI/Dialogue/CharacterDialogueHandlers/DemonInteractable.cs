using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static Unity.Burst.Intrinsics.Arm;

public class DemonInteractable : DialogueInteractable
{
    public override void EndDialogue()
    {
        base.EndDialogue();

        GameObject.FindObjectOfType(typeof(DemonScript)).GetComponent<NavMeshAgent>().enabled = true;
        GameObject.FindObjectOfType(typeof(DemonScript)).GetComponent<Animator>().SetBool("Walk", true);

        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>().isFightingDemon = true;

        // add code here
    }
}
