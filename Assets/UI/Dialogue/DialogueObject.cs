using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class DialogueObject : ScriptableObject
{
    [TextArea]
    public string[] dialogueText;
    public AudioClip[] dialogueAudio;
}