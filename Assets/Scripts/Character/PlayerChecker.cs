using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Cinemachine;

public class PlayerChecker : MonoBehaviour
{
    public GameObject playerPrefab;
    public CinemachineVirtualCamera cam;
    public static PlayerChecker instance;
    public static bool firstSpawned = false;
    public Inventory defaultInventory = new();

    // Use this for initialization
    void Awake()
    {
        var cams = transform.parent.GetComponentsInChildren<CinemachineVirtualCamera>();
        instance = this;
        if (PlayerVars.instance == null)
        {
            // Disable all secondary cameras in scene
            foreach (CinemachineVirtualCamera cam in cams)
            {
                cam.gameObject.SetActive(false);
            }
            // Re-enable the main camera
            cam.gameObject.SetActive(true);
            GameObject player = Instantiate(playerPrefab, transform.position, Quaternion.identity);
            PlayerVars vars = player.GetComponent<PlayerVars>();

            // Load save data
            if (GameSaver.loading && !GameSaver.currData.emptySave)
            {
                GameSaver.currData.player.SetValues(player);
                if (GameSaver.currData.quitWhileClearing)
                {
                    vars.SetSpawnPos(transform.position);
                }
                player.transform.position = vars.GetSpawnPos();
                GameSaver.loading = false;
            }
            else
            {
                vars.SetSpawnPos(transform.position);
                vars.inventory.copy(defaultInventory);
                vars.SaveInventory();
            }

            firstSpawned = true;
            cam.Follow = player.transform;
            player.GetComponent<PlayerController>().virtualCamera = cam;
            player.GetComponent<PlayerController>().levelZoom = cam.m_Lens.OrthographicSize;
        }
        else
        {
            // Disable all secondary cameras in the scene
            foreach (CinemachineVirtualCamera cam in cams)
            {
                cam.gameObject.SetActive(false);
            }
            if (!firstSpawned)
            {
                PlayerVars.instance.SetSpawnPos(transform.position);
                firstSpawned = true;
            }
            PlayerVars.instance.Reset(PlayerVars.instance.GetSpawnPos());
            PlayerController.instance.virtualCamera = cam;
            // Re-enable the main camera
            cam.gameObject.SetActive(true);
            cam.Follow = PlayerVars.instance.transform;
            PlayerController.instance.levelZoom = cam.m_Lens.OrthographicSize;
        }
    }
}
