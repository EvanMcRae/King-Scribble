using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenObjDestroyer : MonoBehaviour
{
    [SerializeField] private SoundPlayer soundPlayer;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Pen"))
        {
            Line line = collision.gameObject.GetComponent<Line>();
            if (!line.deleted)
            {
                line.deleted = true;
                soundPlayer.PlaySound("Ink.Zap");
                Destroy(collision.gameObject);
            }
        }
        else if (collision.gameObject.CompareTag("TempObj"))
        {
            Destroy(collision.gameObject);
        }
    }
}
