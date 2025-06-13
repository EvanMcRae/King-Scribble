using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PenMeter : MonoBehaviour
{
    [SerializeField] private Image sprite, monitor;

    void Start()
    {
        DrawManager.GetTool(ToolType.Pen)._fuelEvent += UpdateSprite;
        DrawManager.GetTool(ToolType.Pen)._tempFuelEvent += UpdateMonitor;
    }

    void UpdateSprite(float penPercent)
    {
        sprite.fillAmount = penPercent;
    }

    void UpdateMonitor(float penPercent)
    {
        monitor.fillAmount = penPercent;
    }

    void OnDestroy()
    {
        DrawManager.GetTool(ToolType.Pen)._fuelEvent -= UpdateSprite;
        DrawManager.GetTool(ToolType.Pen)._tempFuelEvent -= UpdateMonitor;
    }
}
