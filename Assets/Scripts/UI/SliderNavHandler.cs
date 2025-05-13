using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderNavHandler : MonoBehaviour
{
    public Navigation nav;
    public bool navOn = true;
    [SerializeField] private Image image;
    [SerializeField] private Sprite inactive, active;

    public void Start()
    {
        nav = GetComponent<Slider>().navigation;
    }

    public void ToggleNav()
    {
        navOn = !navOn;
        if (navOn)
        {
            GetComponent<Slider>().navigation = nav;
            image.sprite = inactive;
        }
        else
        {
            Navigation newNav = nav;
            newNav.selectOnLeft = null;
            newNav.selectOnRight = null;
            GetComponent<Slider>().navigation = newNav;
            image.sprite = active;
        }
    }

    public void EnableNav()
    {
        navOn = true;
        GetComponent<Slider>().navigation = nav;
    }
}
