using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ToolDatabase", menuName = "ScriptableObjects/ToolDatabase", order = 1)]
public class ToolDatabase : ScriptableObject
{
    // This should be ordered EXACTLY CORRESPONDING TO ToolType enum entries, offset by 1 for the None type.
    public List<Tool> tools = new();
}
