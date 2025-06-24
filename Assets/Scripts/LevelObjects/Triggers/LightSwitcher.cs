using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject _lightPref;
    [SerializeField] private Light2D _global;

    [Tooltip("True: disable global light; False: (re)enable global light.")]
    [SerializeField] private bool _disableGlobal;
    [Tooltip("True: attach light prefab to player; False: remove light prefab from player (if applicable).")]
    [SerializeField] private bool _attachLight;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Transform player = collision.gameObject.transform.root;
            Light2D light;

            if (_disableGlobal)
            {
                _global.enabled = false;
            }

            else
            {
                _global.enabled = true;
            }

            if (_attachLight)
            {
                if (!player.GetComponentInChildren<Light2D>())
                {
                    Instantiate(_lightPref, player);
                }
            }

            else
            {
                if (light = player.GetComponentInChildren<Light2D>())
                {
                    Destroy(light);
                }
            }
        }
    }
}
