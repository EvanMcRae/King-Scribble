using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Collectible : MonoBehaviour
{
    private bool beenCollected = false;
    public UnityEvent pickup;
    // Called on collision with the player, generic "pickup" function
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !beenCollected)
        {
            beenCollected = true;
            pickup.Invoke();
            OnPickup(other);
            Destroy(gameObject);
        }
    }
    // Function for specific behaviors in each different collectible type
    public virtual void OnPickup(Collider2D player)
    {
        Debug.Log("Generic Pickup");
    }
}
