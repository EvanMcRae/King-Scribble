using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EBInkPipe : MonoBehaviour
{
    [SerializeField] private Transform start;
    [SerializeField] private Transform active;
    [SerializeField] private Transform end;
    [SerializeField] private GameObject inkfall;
    private GameObject physics;
    [SerializeField] private Sprite broken;
    [SerializeField] private ParticleSystem break_particles;
    [SerializeField] private SoundPlayer sound_player;
    [SerializeField] private SoundPlayer pipeSoundPlayer;
    [SerializeField] private EBInkPipe other;
    private Animator anim;
    public bool is_enabled = true, is_active = false, is_busy = false;
    private static int inkSoundPlayers = 0;

    void Start()
    {
        physics = inkfall.transform.GetChild(0).gameObject;
        anim = GetComponent<Animator>();
        if (!is_active)
        {
            inkfall.transform.position = start.position;
            physics.SetActive(false);
        }
        else
        {
            anim.Play("Pipe_Start");
            inkfall.transform.position = active.position;
            if (inkSoundPlayers == 0)
            {
                if (!sound_player.sources[0].isPlaying)
                    sound_player.PlaySound("Ink.Flood", 0, true);
                AudioManager.instance.StartCoroutine(AudioManager.instance.FadeAudioSource(sound_player.sources[0], 1f, 1f, () => { }));
            }
            inkSoundPlayers++;
        }
        GameManager.ResetAction += FadeOut;
    }

    public void Activate()
    {
        if (is_enabled && !is_active && !is_busy && gameObject.activeInHierarchy)
        {   
            StartCoroutine(Activate_());
        }
    }

    private IEnumerator Activate_()
    {
        is_busy = true;
        anim.Play("Pipe_Start");
        yield return new WaitForSeconds(1.0f);
        inkfall.transform.DOMoveY(active.position.y, 0.5f);
        if (inkSoundPlayers == 0)
        {
            if (!sound_player.sources[0].isPlaying)
                sound_player.PlaySound("Ink.Flood", 0, true);
            AudioManager.instance.StartCoroutine(AudioManager.instance.FadeAudioSource(sound_player.sources[0], 0.25f, 1f, () => { }));
        }
        yield return new WaitForSeconds(0.5f);
        physics.SetActive(true);
        is_active = true;
        is_busy = false;
        inkSoundPlayers++;
    }

    public void Deactivate()
    {
        if (is_enabled && is_active && !is_busy && gameObject.activeInHierarchy)
            StartCoroutine(Deactivate_());
    }

    IEnumerator Deactivate_()
    {
        is_busy = true;
        physics.SetActive(false);
        anim.Play("Pipe_Stop");
        inkfall.transform.DOMoveY(end.position.y, 0.5f);
        yield return new WaitForSeconds(0.5f);
        anim.Play("Pipe_Idle");
        inkSoundPlayers--;
        if (inkSoundPlayers == 0)
        {
            AudioManager.instance.StartCoroutine(AudioManager.instance.FadeAudioSource(sound_player.sources[0], 0.25f, 0f, () => { }));
        }
        inkfall.transform.position = start.transform.position;
        is_active = false;
        is_busy = false;
    }

    public void Break()
    {
        if (is_enabled)
        {
            StopAllCoroutines();
            StartCoroutine(Break_());
        }
    }

    IEnumerator Break_()
    {
        is_enabled = false;
        physics.SetActive(false);
        if (is_active)
        {
            is_active = false;
            // Deactivate, then end sound only if other pipe's ink is not flowing
            inkfall.transform.DOMoveY(end.position.y, 0.5f);
            yield return new WaitForSeconds(0.5f);
            inkSoundPlayers--;
            if (inkSoundPlayers == 0)
            {
                AudioManager.instance.StartCoroutine(AudioManager.instance.FadeAudioSource(sound_player.sources[0], 0.25f, 0f, () => { }));
            }
            inkfall.transform.position = start.transform.position;
        }
        // gameObject.GetComponent<SpriteRenderer>().sprite = broken;
        ParticleSystem fart = Instantiate(break_particles, gameObject.transform.position, Quaternion.identity);
        var farte = fart.shape;
        farte.scale = 4f * Vector3.one;
        anim.Play("Pipe_Broken");
        pipeSoundPlayer.PlaySound("EraserBoss.PipeExplosion");
    }

    public void FadeOut(bool shouldFadeMusic)
    {
        if (sound_player != null)
            AudioManager.instance.StartCoroutine(AudioManager.instance.FadeAudioSource(sound_player.sources[0], 1f, 0f, () => { }));
        if (shouldFadeMusic)
        {
            AudioManager.instance.FadeOutCurrent();
        }
    }

    private void OnDestroy()
    {
        GameManager.ResetAction -= FadeOut;
    }
}
