using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjBoundary : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("TempObj") || other.gameObject.CompareTag("Pen")) Destroy(other.gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("TempObj") || other.CompareTag("Pen")) Destroy(other.gameObject);
    }
}
