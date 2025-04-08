using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class Settings
{
    public int quality = 3;
    public bool fullScreen = true;
    public bool vSync = false;
    public float xRes = 1f, yRes = 1f;
    public float musicVolume = 50f;
    public float sfxVolume = 50f;
    public bool audioMute = false;
}