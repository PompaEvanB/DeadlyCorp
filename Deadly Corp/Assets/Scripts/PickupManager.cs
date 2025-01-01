using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PickupManager : MonoBehaviour
{
    [Header("Basics")]
    public float pickupRange;
    public LayerMask itemLayers;
    bool holdingItem;

    [Header("Events")]
    public UnityEvent onPickup;
    public UnityEvent onDrop;

    [Header("References")]
    Transform selectedItem;
    Transform heldItem;
    [SerializeField]
    Transform cam;
    [SerializeField]
    Transform holdPoint;

    void Update()
    {
        HandleInput();
        HandleSelection();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!holdingItem && selectedItem != null)
            {
                PickupItem();
            }
            else if (holdingItem)
            {
                DropItem();
            }
        }
    }

    void HandleSelection()
    {
        RaycastHit hit;
        Physics.Raycast(cam.position, cam.forward, out hit, pickupRange, itemLayers);
        selectedItem = hit.transform;
    }

    void PickupItem()
    {
        onPickup.Invoke();

        //We are now holding an item, not just selecting one
        holdingItem = true;
        heldItem = selectedItem;
        selectedItem = null;

        //Moving item to hand
        heldItem.parent = holdPoint;
        heldItem.localPosition = holdPoint.localPosition;
    }


    void DropItem()
    {
        onDrop.Invoke();

        holdingItem = false;
        heldItem.parent = null;
    }

}
