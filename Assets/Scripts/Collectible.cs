using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    private bool beenCollected = false;
    // Called on collision with the player, generic "pickup" function
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player" && !beenCollected)
        {
            beenCollected = true;
            OnPickup(other);
            Destroy(this.gameObject);
        }
    }
    // Function for specific behaviors in each different collectible type
    public virtual void OnPickup(Collider2D player)
    {
        Debug.Log("Generic Pickup");
    }
}
