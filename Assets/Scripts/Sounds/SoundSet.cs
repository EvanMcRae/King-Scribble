using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Sound Set", menuName = "AudioAssets/Sound Set")]
public class SoundSet : SoundPlayable
{
    public List<AudioClip> clips;
    private int lastClip;
    public override AudioClip GetClip()
    {
        lastClip = Random.Range(0, clips.Count);
        return clips[lastClip];
    }
    public float length()
    {
        return clips[lastClip].length;
    }
}