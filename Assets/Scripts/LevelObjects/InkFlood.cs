using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkFlood : MonoBehaviour
{
    public float floodSpeed;
    public Transform destination;
    public bool flooding = false;
    public float speedUpPoint = 0f; // (Optional) the y-value of the point at which the ink will double its speed permanently
    private bool hasSped = false; // If the ink has reached the "speed up" point
    private bool catchUp = false; // If the ink is currently "catching up"
    public float maxDist = 10f; // How far the player can move away from the ink before it speeds up to catch him

    public void StartFlood()
    {
        flooding = true;
        // Possibly add: if not first time in the level (must implement into save functionality)
        if (speedUpPoint != 0)
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
            if (!hasSped && transform.position.y >= speedUpPoint) // If the "speed up point" has been reached, speed up permanently
            {
                floodSpeed *= 2;
                hasSped = true;
            }

            if (hasSped && !catchUp && PlayerVars.instance.transform.position.y >= transform.position.y + maxDist) // If the player is too far ahead, double speed until caught up
            {
                floodSpeed *= 2;
                catchUp = true;
            }

            if (catchUp && PlayerVars.instance.transform.position.y < transform.position.y + maxDist) // If caught up, set the speed back to normal
            {
                floodSpeed /= 2;
                catchUp = false;
            } 

            transform.position = Vector2.MoveTowards(transform.position, destination.position, floodSpeed * Time.fixedDeltaTime);
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
