using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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
    // Start is called before the first frame update
    void Start()
    {
        Physics2D.IgnoreCollision(_base.GetComponent<Collider2D>(), _top.GetComponent<Collider2D>());
        _top.GetComponent<Rigidbody2D>().mass = req_weight;
    }

    // Trigger - button has an extra Collider2D at the very bottom of the base. If the top part of the button enters this collider, the button is fully pressed.

    // Trigger on "full press" - when the top of the button is at its lowest possible point (or within a small window)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Button" && !pressed)
        {
            pressed = true;
            // Debug.Log("Pressed");
            on_press.Invoke();
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
        if (other.tag == "Button" && pressed)
        {
            pressed = false;
            // Debug.Log("Released");
            on_release.Invoke();
        } 
    }
}
