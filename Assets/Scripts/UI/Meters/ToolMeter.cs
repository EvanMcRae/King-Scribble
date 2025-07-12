using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ToolMeter : MonoBehaviour
{
    protected ToolType _toolType;

    [SerializeField] protected Animator anim;

    [SerializeField] protected Image meter, monitor;

    protected virtual void UpdateMeter(float percent) { }

    protected virtual void UpdateMonitor(float percent) { }

    protected virtual void ReleaseCursor() { }

    protected virtual void Start()
    {
        DrawManager.GetTool(_toolType)._fuelEvent += UpdateMeter;
        DrawManager.GetTool(_toolType)._tempFuelEvent += UpdateMonitor;
        DrawManager.GetTool(_toolType)._releaseCursor += ReleaseCursor;
    }

    protected virtual void OnDestroy()
    {
        DrawManager.GetTool(_toolType)._fuelEvent -= UpdateMeter;
        DrawManager.GetTool(_toolType)._tempFuelEvent -= UpdateMonitor;
        DrawManager.GetTool(_toolType)._releaseCursor -= ReleaseCursor;
    }
}
