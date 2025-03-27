using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScribbleMeter : MonoBehaviour
{
    [SerializeField] private Animator anim;

    void Start()
    {
        PlayerVars.instance.doodleEvent += UpdateSprite;
    }

    void UpdateSprite(float doodlePercent)
    {
        anim.SetFloat("Fullness", doodlePercent);
    }

    void OnDestroy()
    {
        PlayerVars.instance.doodleEvent -= UpdateSprite;
    }
}
