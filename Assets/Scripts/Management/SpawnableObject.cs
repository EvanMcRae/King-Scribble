using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnableObject : MonoBehaviour
{
    [SerializeField] private SoundPlayer soundPlayer;
    [SerializeField] private SoundClip landingSound;
    private bool hasLanded;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasLanded && collision.gameObject.layer != LayerMask.GetMask("NoDraw"))
        {
            if (collision.contactCount > 0)
            {
                // Only play landing sound upon landing on the bottom face
                if (collision.GetContact(0).point.y < transform.position.y)
                    soundPlayer.PlaySound(landingSound);

                hasLanded = true;
            }
        }
    }
}
