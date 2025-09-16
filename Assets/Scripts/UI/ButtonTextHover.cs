using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class HoverOpacityControl : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    // Attach this script to a button to fade its text opacity in and out :D
    public TextMeshProUGUI targetText;
    [SerializeField] float hoverAlpha; // desired opacity when hovered
    private float originalAlpha;

    void Start()
    {
        if (targetText != null)
        {
            originalAlpha = targetText.color.a;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log("Entered Button Area");
        Activate();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //Debug.Log("Exited Button Area");
        Deactivate();
    }

    public void OnSelect(BaseEventData eventData)
    {
        Activate();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        Deactivate();
    }

    // Activates properties of button
    private void Activate()
    {
        if (targetText != null)
        {
            Color currentColor = targetText.color;
            targetText.color = new Color(currentColor.r, currentColor.g, currentColor.b, hoverAlpha);
        }
    }

    // Deactivates properties of button
    private void Deactivate()
    {
        if (targetText != null)
        {
            Color currentColor = targetText.color;
            targetText.color = new Color(currentColor.r, currentColor.g, currentColor.b, originalAlpha);
        }
    }
}