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
    [SerializeField] private Sprite broken;
    [SerializeField] private ParticleSystem break_particles;
    [SerializeField] private SoundPlayer sound_player;
    [SerializeField] private SoundPlayer pipeSoundPlayer;
    [SerializeField] private EBInkPipe other;
    private Animator anim;
    private bool is_enabled = true;
    private bool is_active = false;

    void Start()
    {
        inkfall.transform.position = start.position;
        anim = GetComponent<Animator>();
    }

    public void Activate()
    {
        if (is_enabled && !is_active)
        {   
            StartCoroutine(Activate_());
        }
    }

    private IEnumerator Activate_() {
        anim.Play("Pipe_Start");
        yield return new WaitForSeconds(1.0f);
        inkfall.transform.DOMoveY(active.position.y, 0.5f);
        is_active = true;
        sound_player.PlaySound("Ink.Flood", 1, true);
    }

    public void Deactivate()
    {
        if (is_active)
            StartCoroutine(Deactivate_());
    }

    IEnumerator Deactivate_()
    {
        anim.Play("Pipe_Stop");
        inkfall.transform.DOMoveY(end.position.y, 0.5f);
        yield return new WaitForSeconds(0.5f);
        anim.Play("Pipe_Idle");
        sound_player.EndSound("Ink.Flood");
        inkfall.transform.position = start.transform.position;
        is_active = false;
    }

    public void Break()
    {
        StartCoroutine(Break_());
    }

    IEnumerator Break_()
    {
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
        is_enabled = false;
    }
}
