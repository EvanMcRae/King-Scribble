using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class GenericCamEvent : MonoBehaviour
{
    [SerializeField] private CinemachineCamera main_cam; // The main camera to switch from/to (usually the player's main virtual camera)
    [SerializeField] private CinemachineCamera cine_cam; // The "cinematic" camera to switch to/from (ideally one anchored on the main area of cinematic focus)
    [SerializeField] private float cine_time; // The time spent lingering on the cinematic camera before switching back to the main camera

    public void CamEvent() {GameManager.instance.SwitchCameras(main_cam, cine_cam, cine_time);}
}
