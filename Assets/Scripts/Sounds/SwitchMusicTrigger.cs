using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchMusicTrigger : MonoBehaviour
{
    public MusicClip newTrack;
    private MusicClip oldTrack;
    [SerializeField] private bool setsOld = false;
    [SerializeField] private bool sameArea = true;
    [SerializeField] private float duration = 1;

    void Awake()
    {
        if (setsOld)
        {
            GameManager.ResetAction += OnReset;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            oldTrack = AudioManager.instance.currentSong;
            if (newTrack != null)
            {
                AudioManager.instance.ChangeBGM(newTrack, sameArea ? AudioManager.instance.currentArea : newTrack.area, duration);
            }
            else
            {
                AudioManager.instance.FadeOutCurrent(duration);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (setsOld && other.CompareTag("Player"))
        {
            if (newTrack != null)
            {
                if (oldTrack != null)
                {
                    AudioManager.instance.ChangeBGM(oldTrack, AudioManager.instance.currentArea);
                }
            }
            else
            {
                AudioManager.instance.FadeInCurrent(duration);
            }
        }
    }

    public void OnReset(bool _)
    {
        GameManager.ResetAction -= OnReset;
        DisableSettingOldMusic();
    }

    public void DisableSettingOldMusic()
    {
        setsOld = false;
    }
}
