using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraSwitcher : MonoBehaviour
{
    public CinemachineVirtualCamera cam;
    public bool setActive; // 0 to deactivate specified camera, 1 to activate
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (!setActive) GameManager.instance.DeactivateCamera(cam);
            else GameManager.instance.SetCamera(cam);
        }
    }
}
