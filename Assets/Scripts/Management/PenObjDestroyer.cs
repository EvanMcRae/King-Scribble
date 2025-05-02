using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenObjDestroyer : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("TempObj") || collision.gameObject.CompareTag("Pen"))
            Destroy(collision.gameObject);
    }
}
