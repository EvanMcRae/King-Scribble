using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkFlood : MonoBehaviour
{
    public float floodSpeed;
    public Transform destination;
    public bool flooding = false;
    public float speedUpPoint;
    private bool hasSped = false;
    private bool catchUp = false;
    public void StartFlood()
    {
        flooding = true;
        // Possibly add: if not first time in the level (must implement into save functionality)
        floodSpeed /= 2f; // Initial speed half of final
    }

    public void StopFlood()
    {
        flooding = false;
    }

    void FixedUpdate()
    {
        if (flooding)
        {
            transform.position = Vector2.MoveTowards(transform.position, destination.position, floodSpeed * Time.fixedDeltaTime);

            if (transform.position.y >= speedUpPoint && !hasSped)
            {
                floodSpeed *= 2;
                hasSped = true;
            }

            if (PlayerVars.instance.transform.position.y >= transform.position.y + 5f && !catchUp)
            {
                floodSpeed *= 2;
                catchUp = true;
            }

            if (PlayerVars.instance.transform.position.y < transform.position.y + 5f && catchUp) 
            {
                floodSpeed /= 2;
                catchUp = false;
            } 
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && collision.gameObject.name != "LandCheck") // != LandCheck to prevent early deaths due to the land check object mistakenly colliding with the ink
        {
            GetComponent<BuoyancyEffector2D>().density = 0.1f;
            GameManager.instance.Reset();
        }
    }
}
