using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface ITargetable
{


     // as target
     ulong NetworkObjectId { get; }
     Transform transform { get; }



     // about damage
     bool isDamageable { get; }




}
