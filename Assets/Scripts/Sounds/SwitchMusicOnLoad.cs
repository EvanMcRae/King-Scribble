using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchMusicOnLoad : MonoBehaviour
{
    public MusicClip newTrack;
    public AudioManager.GameArea newArea;
    private AudioManager theAM;

    // Start is called before the first frame update
    void Start()
    {
        PlayMusic();
    }

    public void PlayMusic()
    {
        if (newTrack != null)
        {
            theAM = FindFirstObjectByType<AudioManager>();
            theAM.ChangeBGM(newTrack, newArea);
        }
    }
}
