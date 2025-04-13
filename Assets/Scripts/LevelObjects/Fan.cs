using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fan : MonoBehaviour
{
    public float fanForce;
    
    void OnTriggerStay2D(Collider2D collision)
    {
        collision.transform.root.GetComponent<Rigidbody2D>().AddForce(transform.up * fanForce);
    }
}
