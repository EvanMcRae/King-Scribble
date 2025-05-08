using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAmbientSound : MonoBehaviour
{
    [SerializeField] private SoundClip sound;
    [SerializeField] private float volume = 1;
    [SerializeField] private bool loop = true;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<SoundPlayer>().PlaySound(sound, volume, loop);
    }
}
