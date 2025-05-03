using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderNavHandler : MonoBehaviour
{
    public Navigation nav;
    public bool navOn = true;

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
        }
        else
        {
            Navigation newNav = nav;
            newNav.selectOnLeft = null;
            newNav.selectOnRight = null;
            GetComponent<Slider>().navigation = newNav;
        }
    }

    public void EnableNav()
    {
        navOn = true;
        GetComponent<Slider>().navigation = nav;
    }
}
