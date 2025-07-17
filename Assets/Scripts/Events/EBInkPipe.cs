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
    private bool is_enabled = true;
    public bool is_active = false, is_busy = false;

    void Start()
    {
        physics = inkfall.transform.GetChild(0).gameObject;
        if (!is_active)
        {
            inkfall.transform.position = start.position;
            physics.SetActive(false);
        }
        anim = GetComponent<Animator>();
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
        sound_player.PlaySound("Ink.Flood", 1, true);
        yield return new WaitForSeconds(0.5f);
        physics.SetActive(true);
        is_active = true;
        is_busy = false;
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
        sound_player.EndSound("Ink.Flood");
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
            // Deactivate, then end sound only if other pipe's ink is not flowing
            inkfall.transform.DOMoveY(end.position.y, 0.5f);
            yield return new WaitForSeconds(0.5f);
            if (!other.is_active || !other.is_enabled)
                sound_player.EndSound("Ink.Flood");
            inkfall.transform.position = start.transform.position;
        }
        is_active = false;
        // gameObject.GetComponent<SpriteRenderer>().sprite = broken;
        ParticleSystem fart = Instantiate(break_particles, gameObject.transform.position, Quaternion.identity);
        var farte = fart.shape;
        farte.scale = 4f * Vector3.one;
        anim.Play("Pipe_Broken");
        pipeSoundPlayer.PlaySound("EraserBoss.PipeExplosion");
    }
}
