﻿using UnityEngine;
using System.Collections;
using Cinemachine;

public class PlayerChecker : MonoBehaviour
{
    public GameObject playerPrefab;
    public CinemachineVirtualCamera cam;

    // Use this for initialization
    void Awake()
    {
        if (PlayerVars.instance == null)
        {
            GameObject player = Instantiate(playerPrefab, transform.position, Quaternion.identity);
            cam.Follow = player.transform;
            player.GetComponent<PlayerController>().virtualCamera = cam;
            player.GetComponent<PlayerController>().levelZoom = cam.m_Lens.OrthographicSize;
        }
        else
        {
            PlayerVars.instance.Reset(transform.position);
            PlayerController.instance.virtualCamera = cam;
            cam.Follow = PlayerVars.instance.transform;
            PlayerController.instance.levelZoom = cam.m_Lens.OrthographicSize;
        }
    }
}
