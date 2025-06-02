using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lvl6ButtonEvent : MonoBehaviour
{
    [SerializeField] private PhysicsButton _mid_button;
    private int _counter = 0;
    void Start()
    {
        _mid_button.Deactivate();
    }
    public void ButtonPress()
    {
        _counter++;
        if (_counter >= 2)
        {
            _mid_button.Activate();
        }
    }
}
