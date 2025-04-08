using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RandomMusicOnLoad : SwitchMusicOnLoad
{
    public List<MusicClip> clips;

    // Start is called before the first frame update
    void Start()
    {
        int clip = Random.Range(0, clips.Count);
        newTrack = clips[clip];
        PlayMusic();
    }
}