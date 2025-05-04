using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButtonAlternator : MonoBehaviour, ISelectHandler
{
    [SerializeField] private GameObject UpperButton;
    [SerializeField] private GameObject LowerButton;

    public void OnSelect(BaseEventData eventData)
    {
        Navigation nav = UpperButton.GetComponent<Selectable>().navigation;
        nav.selectOnDown = LowerButton.GetComponent<Selectable>();
        UpperButton.GetComponent<Selectable>().navigation = nav;
    }
}