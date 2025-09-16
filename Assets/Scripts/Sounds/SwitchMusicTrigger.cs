using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchMusicTrigger : MonoBehaviour
{
    public MusicClip newTrack;
    private MusicClip oldTrack;
    private AudioManager theAM;
    [SerializeField] private bool setsOld = false;
    [SerializeField] private bool sameArea = true;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && newTrack != null)
        {
            theAM = FindFirstObjectByType<AudioManager>();
            oldTrack = theAM.currentSong;
            theAM.ChangeBGM(newTrack, sameArea ? theAM.currentArea : newTrack.area);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (setsOld && other.CompareTag("Player") && oldTrack != null)
        {
            theAM = FindFirstObjectByType<AudioManager>();
            theAM.ChangeBGM(oldTrack, theAM.currentArea);
        }
    }
}
