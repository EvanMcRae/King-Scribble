using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Music Clip", menuName = "AudioAssets/Music Clip")]
public class MusicClip : SoundPlayable
{
    public AudioClip clip;
    public float BPM;
    public int timeSignature, timeSignatureBottom = 4, barsLength, preEntryBars = 0;
    public AudioManager.GameArea area;
    public override AudioClip GetClip()
    {
        return clip;
    }
    public float length()
    {
        return clip.length;
    }
}
