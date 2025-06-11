using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PenMeter : MonoBehaviour
{
    [SerializeField] private Image sprite, monitor;

    void Start()
    {
        PlayerVars.instance._pen._fuelEvent += UpdateSprite;
        PlayerVars.instance._pen._tempFuelEvent += UpdateMonitor;
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
        PlayerVars.instance._pen._fuelEvent -= UpdateSprite;
        PlayerVars.instance._pen._tempFuelEvent -= UpdateMonitor;
    }
}
