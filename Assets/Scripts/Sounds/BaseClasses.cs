using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundNode : ScriptableObject
{
}

public abstract class SoundPlayable : SoundNode
{
    public abstract AudioClip GetClip();
}