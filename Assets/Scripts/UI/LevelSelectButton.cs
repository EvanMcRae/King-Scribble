using UnityEngine;
using UnityEngine.UI;

public class LevelSelectButton : MonoBehaviour
{
    [SerializeField] private Sprite normal, selected;

    private void Awake()
    {
        SetButtonActive(false);
    }

    public void SetButtonActive(bool active)
    {
        GetComponent<Image>().sprite = active ? selected : normal;
        GetComponent<Button>().interactable = active;
    }
}