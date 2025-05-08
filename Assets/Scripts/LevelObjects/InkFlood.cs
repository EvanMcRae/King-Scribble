using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkFlood : MonoBehaviour
{
    [SerializeField] private float floodSpeed;
    public void SetSpeed(float speed) {floodSpeed = speed;}
    public Transform[] destinations; // There MUST be at least one
    public bool flooding = false;
    public bool catchUp_enabled = true; // Set to false if the ink is not "chasing" the player
    public bool speedUp_enabled = true; // Set to false if no "speed up point" is desired
    public float speedUpPoint = 0f; // (Optional) the y-value of the point at which the ink will double its speed permanently
    private bool hasSped = false; // If the ink has reached the "speed up" point
    private bool catchUp = false; // If the ink is currently "catching up"
    public float maxDist = 10f; // How far the player can move away from the ink before it speeds up to catch him
    private int curDest = 0;

    // OPTIONAL - if you want to wait X seconds before the flood starts
    public bool floodWait = false;
    public float waitTime = 0f;
    private bool waiting = false;
    public float killThreshold = 0.5f;

    public SoundPlayer soundPlayer;

    public void SetDest(int dest_index) {curDest = dest_index;}
    public void NextDest() {if (destinations.Length > curDest - 1) curDest++;}
    public void PrevDest() {if (curDest > 0) curDest--;}

    public void StartFlood()
    {

        flooding = true;
        // Possibly add: if not first time in the level (must implement into save functionality)
        if (speedUp_enabled)
            floodSpeed /= 2f; // Initial speed half of final
        if (floodWait) 
        {
            waiting = true;
            StartCoroutine(WaitTime(waitTime));
        }
        else
        {
            if (soundPlayer != null && !soundPlayer.sources[0].isPlaying)
            {
                soundPlayer.PlaySound("Ink.Flow", 1, true);
            }
        }
    }

    IEnumerator WaitTime(float wait_time)
    {
        waiting = true;
        yield return new WaitForSeconds(wait_time);
        waiting = false;
        if (soundPlayer != null && !soundPlayer.sources[0].isPlaying)
        {
            soundPlayer.PlaySound("Ink.Flow", 1, true);
        }
    }

    public void StopFlood()
    {
        flooding = false;
        if (soundPlayer != null)
            soundPlayer.EndSound("Ink.Flow");
    }

    void FixedUpdate()
    {
        if (flooding && !waiting)
        {
            // If the "speed up point" has been reached, speed up permanently
            if (speedUp_enabled && !hasSped && transform.position.y >= speedUpPoint)
            {
                floodSpeed *= 2;
                hasSped = true;
            }
            // If the player is too far ahead, double speed until caught up
            if (catchUp_enabled && ((speedUp_enabled && hasSped) || !speedUp_enabled) && !catchUp && PlayerVars.instance.transform.position.y >= transform.position.y + maxDist)
            {
                floodSpeed *= 2;
                catchUp = true;
            }
            // If caught up, set the speed back to normal
            if (catchUp_enabled && catchUp && PlayerVars.instance.transform.position.y < transform.position.y + maxDist)
            {
                floodSpeed /= 2;
                catchUp = false;
            } 

            transform.position = Vector2.MoveTowards(transform.position, destinations[curDest].position, floodSpeed * Time.fixedDeltaTime);
        }

        if (transform.position == destinations[curDest].position) StopFlood();
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (!GameManager.resetting && collision.gameObject.CompareTag("Player") && collision.gameObject.name != "LandCheck") // != LandCheck to prevent early deaths due to the land check object mistakenly colliding with the ink
        {
            PlayerVars.instance.GetComponent<Rigidbody2D>().mass = 10f;
            if (transform.position.y - collision.transform.position.y > killThreshold && !PlayerVars.instance.cheatMode)
            {
                GetComponent<BuoyancyEffector2D>().density = 0.1f;
                GameManager.instance.Reset();
                PlayerVars.instance.GetComponent<Rigidbody2D>().mass = 1f;
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerVars.instance.GetComponent<Rigidbody2D>().mass = 1f;
        }
    }
}
