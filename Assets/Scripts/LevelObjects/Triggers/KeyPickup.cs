using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPickup : Collectible
{   
    [SerializeField] private GameObject door;
    public override void OnPickup(Collider2D player)
    {
        PlayerController.instance.CollectKey();
        door.GetComponent<DoorScript>().removeLock();
    }
}
