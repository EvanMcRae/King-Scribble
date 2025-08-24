using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public AudioSource[] sources;
    public static bool debugSounds = false;

    // Start is called before the first frame update
    void Start()
    {
        sources = transform.GetComponents<AudioSource>();
    }

    void Update()
    {
        // TODO: Add states for this stuff
        //if (GameManager.instance.paused || GameManager.instance.gameOver) return;

        if (!debugSounds) return;
        string text = "";
        for (int index = sources.Length - 1; index >= 0; index--)
        {
            if (sources[index].isPlaying)
                text += index + " " + sources[index].time + "\n";
        }
        if (text != "")
        {
            string f = "";
            if (transform.parent != null)
                f += transform.parent.gameObject.name + ">";
            f += gameObject.name + "\n" + text;
            //Debug.Log(f);
        }
    }

    public void PlaySound(string path, float volume = 1, bool loop = false)
    {
        PlaySound(AudioManager.instance?.FindSound(path), volume, loop);
    }

    public void PlaySound(SoundPlayable clip, float volume = 1, bool loop = false)
    {
        PlaySound(clip.GetClip(), volume, loop);
    }
    
    public void PlaySound(AudioClip clip, float volume = 1, bool loop = false)
    {
        if (clip == null) return;

        foreach (AudioSource source in sources)
        {
            if (source.clip == clip && source.isPlaying)
            {
                if (source.time < 0.2f) return;
                else source.Stop();
            }
        }
        for (int index = sources.Length - 1; index >= 0; index--)
        {
            if (!sources[index].isPlaying)
            {
                sources[index].clip = clip;
                sources[index].loop = loop;
                sources[index].volume = volume;
                sources[index].Play();
                return;
            }
        }
    }

    public void EndSound(string path = null)
    {
        if (path == null)
        {
            foreach (AudioSource source in sources)
            {
                source.Stop();
                source.clip = null;
                source.time = 0;
            }
            return;
        }
        EndSound(AudioManager.instance?.FindSound(path));
    }

    public void EndSound(SoundClip clip) // TODO does not support SoundSet
    {
        EndSound(clip?.GetClip());
    }

    public void EndSound(AudioClip clip = null)
    {
        if (clip == null)
        {
            foreach (AudioSource source in sources)
            {
                source.Stop();
                source.clip = null;
                source.time = 0;
            }
            return;
        }

        foreach (AudioSource source in sources)
        {
            if (source.clip == clip && source.isPlaying)
            {
                source.Stop();
            }
        }
    }

    public void PauseSound(string path = null)
    {
        if (path == null || path == "all")
        {
            foreach (AudioSource source in sources)
            {
                source.Pause();
            }
            return;
        }
        PauseSound(AudioManager.instance?.FindSound(path));
    }

    public void PauseSound(SoundClip clip) // TODO does not support SoundSet
    {
        PauseSound(clip?.GetClip());
    }

    public void PauseSound(AudioClip clip = null)
    {
        if (clip == null)
        {
            foreach (AudioSource source in sources)
            {
                source.Pause();
            }
            return;
        }

        foreach (AudioSource source in sources)
        {
            if (source == null)
                continue;
            if (source.clip == clip && source.isPlaying)
            {
                source.Pause();
            }
        }
    }

    public void UnPauseSound(string path = null)
    {
        if (path == null || path == "all")
        {
            foreach (AudioSource source in sources)
            {
                source.UnPause();
            }
            return;
        }
        UnPauseSound(AudioManager.instance?.FindSound(path));
    }

    public void UnPauseSound(SoundClip clip) // TODO does not support SoundSet
    {
        UnPauseSound(clip?.GetClip());
    }

    public void UnPauseSound(AudioClip clip = null)
    {
        if (clip == null)
        {
            foreach (AudioSource source in sources)
            {
                source.UnPause();
            }
            return;
        }

        foreach (AudioSource source in sources)
        {
            if (source == null)
                continue;
            if (source.clip == clip)
            {
                source.UnPause();
            }
        }
    }

    public void EndAllSounds()
    {
        foreach (AudioSource source in sources)
        {
            if (source.clip != null)
            {
                source.time = 0;
                source.Stop();
                source.clip = null;
            }
            
        }
    }

    public void CleanSounds()
    {
        foreach (AudioSource source in sources)
        {
            if (!source.isPlaying && source.clip != null)
            {
                source.time = 0;
                source.Stop();
                source.clip = null;
            }
        }
    }

    public void SetVolume(float newVolume)
    {
        foreach(AudioSource source in sources)
        {
            source.volume = newVolume;
        }
    }

}
