using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PickupManager>() != null)
        {
            other.GetComponent<PickupManager>().isNearItem = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PickupManager>() != null)
        {
            other.GetComponent<PickupManager>().isNearItem = false;
        }
    }
}
