using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjBoundary : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "TempObj") Destroy(other.gameObject);
    }
}
