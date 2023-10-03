using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemCard")]
public class ItemCard : ScriptableObject
{

     public int id; //WIP
     public string _name; //display name on the ground
     public Sprite image; //WIP, change to 3D later?
     [TextArea()]
     public string description; //when mouse over


     public int healInstantly = 10;






}
