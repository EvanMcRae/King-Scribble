using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Music Category", menuName = "AudioAssets/Music Category")]
public class MusicCategory : SoundNode
{
    [SerializeField] public List<SoundNode> children = new List<SoundNode>();
}