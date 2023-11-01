using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformStayOn : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        other.gameObject.transform.parent.gameObject.transform.SetParent(transform);
    }
    void OnTriggerExit(Collider other)
    {
        other.gameObject.transform.parent.gameObject.transform.SetParent(null);
    }
}
