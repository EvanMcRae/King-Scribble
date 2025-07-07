using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlighterRMBLightTarget : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("HighlighterRMB"))
        {
            collision.gameObject.GetComponent<HighlighterRMBLight>().HitTarget();
        }
    }
}
