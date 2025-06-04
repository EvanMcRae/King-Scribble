using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class MiniEraserAI : MonoBehaviour
{
    private enum State
    {
        Start,
        Searching,
        Following, // eyes moving, ready to erase
        Attacking,
        Erasing,
        Dizzied,
        Damaged,
        Nothing
    }

    void Start()
    {
        
    }
}