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

    void Update()
    {
        if (flooding)
            transform.position = Vector2.MoveTowards(transform.position, destination.position, floodSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            GetComponent<BuoyancyEffector2D>().density = 0.1f;
            // collision.gameObject.transform.root.GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, -500f));
            GameManager.instance.Reset();
        }
    }
}
