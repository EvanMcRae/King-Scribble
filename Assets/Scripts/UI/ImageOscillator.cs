using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageOscillator : MonoBehaviour
{
    [SerializeField] private float amplitude = 0, period = 0;
    [SerializeField] private bool local = true;
    [SerializeField] private Vector3 pos;

    private void Start()
    {
        pos = local ? transform.localPosition : transform.position;
    }

    void Update()
    {
        if (!GameManager.paused)
        {
            Vector3 newPos = pos;
            newPos.y += amplitude * Mathf.Sin(Time.time * period * 2 * Mathf.PI);
            if (local)
                transform.localPosition = newPos;
            else
                transform.position = newPos;
        }
    }
}
