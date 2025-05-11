using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAmbientSound : MonoBehaviour
{
    [SerializeField] private SoundClip sound;
    [SerializeField] private float volume = 1;
    [SerializeField] private bool loop = true;
    [SerializeField] private SoundPlayer soundPlayer;

    private void Awake()
    {
        soundPlayer = GetComponent<SoundPlayer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        soundPlayer.PlaySound(sound, volume, loop);
        AudioManager.instance.StartCoroutine(AudioManager.instance.FadeAudioSource(soundPlayer.sources[0], 0f, volume, () => { }));
        GameManager.ResetAction += FadeOut;
    }

    void FadeOut()
    {
        AudioManager.instance.StartCoroutine(AudioManager.instance.FadeAudioSource(soundPlayer.sources[0], 1f, 0f, () => { }));
    }

    void OnDestroy()
    {
        GameManager.ResetAction -= FadeOut;
    }
}
