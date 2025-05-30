using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;

public class CameraSwitcher : MonoBehaviour
{
    public CinemachineVirtualCamera prev_cam;
    public CinemachineVirtualCamera next_cam;
    public GameObject p_cam_bounds_l;
    public GameObject p_cam_bounds_r;
    public GameObject n_cam_bounds_l;
    public GameObject n_cam_bounds_r;
    private bool hasEntered = false;
    public UnityEvent onEnter;
    public bool copyFollow = false;
    public int newLensSize = 0;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (prev_cam) GameManager.instance.DeactivateCamera(prev_cam);
            if (p_cam_bounds_l) p_cam_bounds_l.SetActive(false);
            if (p_cam_bounds_r) p_cam_bounds_r.SetActive(false);
            if (next_cam) GameManager.instance.SetCamera(next_cam);
            if (n_cam_bounds_l) n_cam_bounds_l.SetActive(true);
            if (n_cam_bounds_r) n_cam_bounds_r.SetActive(true);

            if (!hasEntered)
            {
                hasEntered = true;
                onEnter.Invoke();
            }

            if (copyFollow) next_cam.Follow = prev_cam.Follow;

            if (newLensSize != 0)
            {
                if (next_cam != PlayerController.instance.virtualCamera)
                    next_cam.m_Lens.OrthographicSize = newLensSize;
                else
                    PlayerController.instance.levelZoom = newLensSize;
            }
        }
    }
}
