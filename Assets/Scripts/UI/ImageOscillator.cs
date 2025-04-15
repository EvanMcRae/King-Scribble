using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageOscillator : MonoBehaviour
{
    [SerializeField] private float amplitude = 0, period = 0;
    
    void Update()
    {
        Vector3 pos = transform.localPosition;
        pos.y += amplitude * Mathf.Sin(Time.time * period * 2 * Mathf.PI);
        transform.localPosition = pos;
    }
}
