using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public AudioMixer musicMixer, sfxMixer;
    public AudioMixerGroup musicMixerGroup;
    public MusicClip currentSong = null;
    public GameArea currentArea;

    private int activePlayer = 0;
    public AudioSource[] BGM1, BGM2;
    private readonly IEnumerator[] fader = new IEnumerator[2];
    public float musicVolume = 1.0f, sfxVolume = 10.0f, masterVolume = 10f, targetSFXVolume= -80.0f, actualSFXVolume = -80.0f;

    //Note: If the volumeChangesPerSecond value is higher than the fps, the duration of the fading will be extended!
    private readonly int volumeChangesPerSecond = 15;

    public float fadeDuration = 1.0f;
    private float loopPointSeconds;
    private bool firstSet = true;
    private bool firstSongPlayed = false;
    public bool paused = false;

    public SoundCategory soundDatabase;
    public MusicCategory musicDatabase;

    private int currentTime = 0;

    /// <summary>
    /// List of all different game areas that may have different sets of music
    /// </summary>
    public enum GameArea
    {
        CURRENT, MENU, LEVEL
    }

    /// <summary>
    /// Set up the AudioSources
    /// </summary>
    private void Awake()
    {
        AudioSettings.Reset(AudioSettings.GetConfiguration());

        if (FindObjectsByType<AudioManager>(FindObjectsSortMode.None).Length > 1)
        {
            instance = null;
            Destroy(gameObject);
            return;
        }

        // Generate two AudioSource lists
        BGM1 = new AudioSource[2]{
            gameObject.AddComponent<AudioSource>(),
            gameObject.AddComponent<AudioSource>()
        };

        BGM2 = new AudioSource[2]{
            gameObject.AddComponent<AudioSource>(),
            gameObject.AddComponent<AudioSource>()
        };

        // Set default values
        foreach (AudioSource s in BGM1)
        {
            s.loop = false;
            s.playOnAwake = false;
            s.volume = 0.0f;
            s.outputAudioMixerGroup = musicMixerGroup;
        }

        foreach (AudioSource s in BGM2)
        {
            s.loop = false;
            s.playOnAwake = false;
            s.volume = 0.0f;
            s.outputAudioMixerGroup = musicMixerGroup;
        }

        // Singleton pattern
        instance = this;
        DontDestroyOnLoad(gameObject);

        if (SettingsManager.currentSettings != null)
        {
            musicVolume = Mathf.Log10(SettingsManager.currentSettings.musicVolume / 100f + 0.00001f) * 20; 
            sfxVolume = Mathf.Log10(SettingsManager.currentSettings.sfxVolume / 100f + 0.00001f) * 20;
            masterVolume = Mathf.Log10(SettingsManager.currentSettings.masterVolume / 100f + 0.00001f) * 20;
        }

        AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;

        sfxMixer.SetFloat("Volume", sfxVolume + masterVolume);
        musicMixer.SetFloat("Volume", musicVolume + masterVolume);

    }

    // Update is called once per frame
    void Update()
    {
        if (instance == null)
        {
            instance = this;
        }

        // Check for desyncing during crossfades
        if ((fader[0] != null || fader[1] != null) && BGM2[activePlayer].isPlaying && BGM1[activePlayer].isPlaying)
        {
            if (firstSet && BGM2[activePlayer].timeSamples != BGM1[activePlayer].timeSamples)
            {
                BGM1[activePlayer].timeSamples = BGM2[activePlayer].timeSamples;
            }
            else if (!firstSet && BGM1[activePlayer].timeSamples != BGM2[activePlayer].timeSamples)
            {
                BGM2[activePlayer].timeSamples = BGM1[activePlayer].timeSamples;
            }
        }

        // Manages looping tracks
        if (firstSet)
        {
            if (BGM1[activePlayer].clip != null && BGM1[activePlayer].time >= loopPointSeconds)
            {
                activePlayer = 1 - activePlayer;
                if (currentSong != null)
                    BGM1[activePlayer].clip = currentSong.GetClip();
                BGM1[activePlayer].volume = 1.0f;
                BGM1[activePlayer].time = 0;
                BGM1[activePlayer].Play();
            }
            currentTime = BGM1[activePlayer].timeSamples;
        }
        else
        {
            if (BGM2[activePlayer].clip != null && BGM2[activePlayer].time >= loopPointSeconds)
            {
                activePlayer = 1 - activePlayer;
                if (currentSong != null)
                    BGM2[activePlayer].clip = currentSong.GetClip();
                BGM2[activePlayer].volume = 1.0f;
                BGM2[activePlayer].time = 0;
                BGM2[activePlayer].Play();
            }
            currentTime = BGM2[activePlayer].timeSamples;
        }

        if (SettingsManager.currentSettings == null)
            return;

        musicVolume = Mathf.Log10(SettingsManager.currentSettings.musicVolume / 100f + 0.00001f) * 20;
        if (GameManager.paused) musicVolume -= 2f;
        sfxVolume = Mathf.Log10(SettingsManager.currentSettings.sfxVolume / 100f + 0.00001f) * 20;
        masterVolume = Mathf.Log10(SettingsManager.currentSettings.masterVolume / 100f + 0.00001f) * 20;

        sfxMixer.SetFloat("Volume", sfxVolume + masterVolume);
        musicMixer.SetFloat("Volume", musicVolume + masterVolume);
        //sfxMixer.SetFloat("MasterVolume", masterVolume);
        //musicMixer.SetFloat("MasterVolume", masterVolume);
    }

    public void ChangeBGM(string musicPath, float duration = 1f)
    {
        MusicClip music = FindMusic(musicPath);
        ChangeBGM(music, duration);
    }

    public void ChangeBGM(string musicPath, string area, float duration = 1f)
    {
        GameArea theArea;
        switch (area.Trim().ToUpper())
        {
            case "CURRENT":
                theArea = currentArea;
                break;
            case "MENU":
                theArea = GameArea.MENU;
                break;
            case "LEVEL":
                theArea = GameArea.LEVEL;
                break;
            default:
                Debug.LogWarning("Invalid area provided! Using current");
                theArea = currentArea;
                break;
        }
        ChangeBGM(FindMusic(musicPath), theArea, duration);
    }

    public void ChangeBGM(string musicPath, GameArea area, float duration = 1f)
    {
        ChangeBGM(FindMusic(musicPath), area, duration);
    }

    public void ChangeBGM(MusicClip music, float duration = 1f)
    {
        ChangeBGM(music, music.area, duration);
    }

    public void ChangeBGM(MusicClip music, GameArea newArea, float duration = 1f)
    {
        // Support cutscenes keeping music area
        if (newArea == GameArea.CURRENT) newArea = currentArea;

        // Carry on music if area has not changed
        bool carryOn = newArea == currentArea;
        currentArea = newArea;

        // Calculate loop point
        loopPointSeconds = 60.0f * (music.barsLength * 4 * music.timeSignature / music.timeSignatureBottom) / music.BPM;

        // Prevent fading the same clip on both players
        if (music == currentSong)
            return;

        // Kill all playing
        foreach (IEnumerator i in fader)
        {
            if (i != null)
            {
                StopCoroutine(i);
            }
        }

        if (firstSet)
        {
            // Fade-out the active play, if it is not silent (eg: first start)
            if (BGM1[activePlayer].volume > 0)
            {
                fader[0] = FadeAudioSource(BGM1[activePlayer], duration, 0.0f, () => { fader[0] = null; });
                StartCoroutine(fader[0]);
            }
            BGM1[1 - activePlayer].Stop();

            // Fade-in the new clip
            BGM2[activePlayer].clip = music.GetClip();
            if (carryOn && BGM1[activePlayer].isPlaying)
            {
                BGM2[activePlayer].timeSamples = BGM1[activePlayer].timeSamples; // syncs up time
            }
            else
            {
                BGM2[activePlayer].timeSamples = 0;
            }
            BGM2[activePlayer].Play();
            if (firstSongPlayed)
            {
                fader[1] = FadeAudioSource(BGM2[activePlayer], duration, 1.0f, () => { fader[1] = null; });
                StartCoroutine(fader[1]);
            }
            else
            {
                BGM2[activePlayer].volume = 1.0f;
            }
        }
        else
        {
            // Fade-out the active play, if it is not silent (eg: first start)
            if (BGM2[activePlayer].volume > 0)
            {
                fader[0] = FadeAudioSource(BGM2[activePlayer], duration, 0.0f, () => { fader[0] = null; });
                StartCoroutine(fader[0]);
            }
            BGM2[1 - activePlayer].Stop();

            // Fade-in the new clip
            BGM1[activePlayer].clip = music.GetClip();
            if (carryOn && BGM2[activePlayer].isPlaying)
            {
                BGM1[activePlayer].timeSamples = BGM2[activePlayer].timeSamples; // Syncs up time
            }
            else
            {
                BGM1[activePlayer].timeSamples = 0;
            }
            BGM1[activePlayer].Play();
            if (firstSongPlayed)
            {
                fader[1] = FadeAudioSource(BGM1[activePlayer], duration, 1.0f, () => { fader[1] = null; });
                StartCoroutine(fader[1]);
            }
            else
            {
                BGM1[activePlayer].volume = 1.0f;
            }
        }

        firstSet = !firstSet;
        firstSongPlayed = true;

        // Set new clip to current song
        currentSong = music;
    }

    /// <summary>
    /// Fades an AudioSource (player) during a given amount of time (duration) to a specific volume (targetVolume)
    /// </summary>
    /// <param name="player">AudioSource to be modified</param>
    /// <param name="duration">Duration of the fading</param>
    /// <param name="targetVolume">Target volume, the player is faded to</param>
    /// <param name="finishedCallback">Called when finshed</param>
    /// <returns></returns>
    public IEnumerator FadeAudioSource(AudioSource player, float duration, float targetVolume, System.Action finishedCallback)
    {
        // Calculate the steps
        int Steps = (int)(volumeChangesPerSecond * duration);
        float StepTime = duration / Steps;
        float StepSize = (targetVolume - player.volume) / Steps;

        // Fade now
        for (int i = 1; i < Steps; i++)
        {
            if (player != null)
                player.volume += StepSize;
            yield return new WaitForSeconds(StepTime);
        }
        // Make sure the targetVolume is set
        if (player != null)
            player.volume = targetVolume;

        // Callback
        finishedCallback?.Invoke();
    }

    public void SetPitch(float pitch)
    {
        musicMixer.SetFloat("Pitch", pitch);
    }

    public IEnumerator PitchDown()
    {
        for (float i = 0; i <= 1; i += 0.05f)
        {
            SetPitch(Mathf.Lerp(1f, 0f, i));
            yield return new WaitForSeconds(0.04f);
        }
    }

    public void FadeOutCurrent(float duration = 1f)
    {
        if (firstSet)
        {
            fader[0] = FadeAudioSource(BGM1[activePlayer], duration, 0.0f, () => { fader[0] = null; });
            StartCoroutine(fader[0]);
        }
        else
        {
            fader[0] = FadeAudioSource(BGM2[activePlayer], duration, 0.0f, () => { fader[0] = null; });
            StartCoroutine(fader[0]);
        }
    }

    public void FadeInCurrent(float duration = 1f)
    {
        if (firstSet)
        {
            fader[0] = FadeAudioSource(BGM1[activePlayer], duration, 1.0f, () => { fader[0] = null; });
            StartCoroutine(fader[0]);
        }
        else
        {
            fader[0] = FadeAudioSource(BGM2[activePlayer], duration, 1.0f, () => { fader[0] = null; });
            StartCoroutine(fader[0]);
        }
    }

    public void PauseCurrent()
    {
        if (firstSet)
        {
            BGM1[activePlayer].Pause();
            if (fader[0] != null)
            {
                BGM2[activePlayer].Pause();
            }
        }
        else
        {
            BGM2[activePlayer].Pause();
            if (fader[0] != null)
            {
                BGM1[activePlayer].Pause();
            }
        }
        paused = true;
    }

    public void UnPauseCurrent()
    {
        if (firstSet)
        {
            BGM1[activePlayer].UnPause();
            if (fader[0] != null)
            {
                BGM2[activePlayer].UnPause();
            }
        }
        else
        {
            BGM2[activePlayer].UnPause();
            if (fader[0] != null)
            {
                BGM1[activePlayer].UnPause();
            }
        }
        paused = false;
    }

    public void Stop()
    {
        foreach (AudioSource source in BGM1)
        {
            source.Stop();
            source.clip = null;
        }
        foreach (AudioSource source in BGM2)
        {
            source.Stop();
            source.clip = null;
        }
        currentSong = null;
        paused = false;
    }

    public AudioClip FindSound(string soundPath)
    {
        List<string> path = new(soundPath.Trim().Split("."));
        return FindSound(soundDatabase, path);
    }

    public AudioClip FindSound(SoundNode current, List<string> path)
    {
        if (current is SoundPlayable playable)
        {
            return playable.GetClip();
        }
        else if (current is SoundCategory category)
        {
            foreach (SoundNode node in category.children)
            {
                if (path.Count > 0 && node.name.ToLower() == path[0].ToLower())
                {
                    // Debug.Log("Found " + path[0]);
                    current = node;
                    path.RemoveAt(0);
                    return FindSound(node, path);
                }
            }
            Debug.LogError("Invalid sound path provided!");
            return null;
        }
        Debug.LogError("Invalid sound path provided!");
        return null;
    }

    public MusicClip FindMusic(string musicPath)
    {
        List<string> path = new(musicPath.Trim().Split("."));
        return FindMusic(musicDatabase, path);
    }

    public MusicClip FindMusic(SoundNode current, List<string> path)
    {
        if (current is MusicClip clip)
        {
            return clip;
        }
        else if (current is MusicCategory category)
        {
            foreach (SoundNode node in category.children)
            {
                if (path.Count > 0 && node.name.ToLower() == path[0].ToLower())
                {
                    current = node;
                    path.RemoveAt(0);
                    return FindMusic(node, path);
                }
            }
            Debug.LogError("Invalid music path provided!");
            return null;
        }
        Debug.LogError("Invalid music path provided!");
        return null;
    }

    public bool OwnsSource(AudioSource source)
    {
        return source == BGM1[0] || source == BGM1[1] || source == BGM2[0] || source == BGM2[1];
    }

    public void PauseEffect(bool active)
    {
        musicMixer.SetFloat("LowPass", active ? 1815.00f : 22000.00f);
    }

    private void OnAudioConfigurationChanged(bool deviceWasChanged)
    {
        if (firstSet)
        {
            BGM1[activePlayer].timeSamples = currentTime;
            if (!paused)
                BGM2[activePlayer].Play();
        }
        else
        {
            BGM2[activePlayer].timeSamples = currentTime;
            if (!paused)
                BGM2[activePlayer].Play();
        }
    }
}