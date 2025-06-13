using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScribbleMeter : MonoBehaviour
{
    [SerializeField] private Animator anim;

    void Start()
    {
        DrawManager.GetTool(ToolType.Pencil)._fuelEvent += UpdateSprite;
    }

    void UpdateSprite(float doodlePercent)
    {
        anim.SetFloat("Fullness", doodlePercent);
    }

    void OnDestroy()
    {
        DrawManager.GetTool(ToolType.Pencil)._fuelEvent -= UpdateSprite;
    }
}
