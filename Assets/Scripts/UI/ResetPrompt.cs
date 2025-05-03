using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ResetPrompt : MonoBehaviour
{
    [SerializeField] private Image outline;
    [SerializeField] private Image fill;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetFill(float amount)
    {
        fill.fillAmount = amount;
    }

    public void SetVisibility(bool visibility)
    {
        outline.enabled = visibility;
        fill.enabled = visibility;
    }
}
