using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkFlood : MonoBehaviour
{
    public float floodSpeed;
    public Transform destination;
    public bool flooding = false;

    public void StartFlood()
    {
        flooding = true;
    }

    public void StopFlood()
    {
        flooding = false;
    }

    void FixedUpdate()
    {
        if (flooding)
            transform.position = Vector2.MoveTowards(transform.position, destination.position, floodSpeed * Time.fixedDeltaTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && collision.gameObject.name != "LandCheck") // != LandCheck to prevent early deaths due to the land check object mistakenly colliding with the ink
        {
            GetComponent<BuoyancyEffector2D>().density = 0.1f;
            // collision.gameObject.transform.root.GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, -500f));
            GameManager.instance.Reset();
        }
    }
}
