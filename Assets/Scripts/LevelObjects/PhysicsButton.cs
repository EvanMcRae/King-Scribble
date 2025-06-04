using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
public class PhysicsButton : MonoBehaviour
{
    public Transform _base;
    public Transform _top;
    public float req_weight;
    private bool pressed = false; // Trure if the button is currently being pressed
    private bool been_pressed = false; // True if the button has been pressed at least once 
    public UnityEvent on_press;
    public UnityEvent on_first_press;
    public UnityEvent on_release;
    public float maxHeight;
    const int NUM_SPRITES = 6;
    public Sprite[] sprites = new Sprite[NUM_SPRITES];
    public SoundPlayer soundPlayer;
    public GameObject cur_ground; // OPTIONAL - current platform/ground object that the button is placed on

    [Serializable]
    private enum Colors
    {
        Red,
        Orange,
        Yellow,
        Green,
        Blue,
        Purple,
        White
    }
    [SerializeField] private Colors _active_color;
    [SerializeField] private Colors _inactive_color;
    private SpriteRenderer _top_sprite;
    private bool _is_active = true;
    [SerializeField] private GameObject _activateAnim;
    [FormerlySerializedAs("b_color")]
    [SerializeField] private Colors _base_color;

    void Start()
    {
        _top_sprite = _top.GetComponent<SpriteRenderer>();
        Physics2D.IgnoreCollision(_base.GetComponent<Collider2D>(), _top.GetComponent<Collider2D>());
        if (cur_ground)
        {
            Physics2D.IgnoreCollision(_top.GetComponent<Collider2D>(), cur_ground.GetComponent<Collider2D>(), false);
            Physics2D.IgnoreCollision(_base.GetComponent<Collider2D>(), cur_ground.GetComponent<Collider2D>());
        }
        _top.GetComponent<Rigidbody2D>().mass = req_weight;
        maxHeight = _top.localPosition.y;
        _top_sprite.sprite = sprites[(int)_base_color];
        if (_activateAnim)
        {
            _activateAnim.SetActive(false);
        }
    }

    [ContextMenu("Update Color")]
    void SetColor()
    {
        _top_sprite.sprite = sprites[(int)_base_color];
    }

    void Update()
    {
        // Force button max height
        if (_top.localPosition.y > maxHeight)
        {
            Vector3 newPos = _top.localPosition;
            newPos.y = maxHeight;
            _top.localPosition = newPos;
        }
    }

    // Trigger - button has an extra Collider2D at the very bottom of the base. If the top part of the button enters this collider, the button is fully pressed.

    // Trigger on "full press" - when the top of the button is at its lowest possible point (or within a small window)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Button") && !pressed && _is_active)
        {
            pressed = true;
            on_press.Invoke();
            soundPlayer.PlaySound("Button.Press");
            if (!been_pressed)
            {
                been_pressed = true;
                on_first_press.Invoke();
            }
        }
    }

    // Trigger on "release" - when weight is removed from the button such that it is no longer fully compressed
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Button") && pressed && _is_active)
        {
            pressed = false;
            on_release.Invoke();
            soundPlayer.PlaySound("Button.Release");
        }
    }

    public void Activate()
    {
        _top_sprite.sprite = sprites[(int)_active_color];
        _is_active = true;
        _activateAnim.SetActive(true);
    }

    public void Deactivate()
    {
        _top_sprite.sprite = sprites[(int)_inactive_color];
        _is_active = false;
        _activateAnim.SetActive(false);
    }
}
