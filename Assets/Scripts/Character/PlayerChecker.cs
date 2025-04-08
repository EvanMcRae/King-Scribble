using UnityEngine;
using System.Collections;
using Cinemachine;

public class PlayerChecker : MonoBehaviour
{
    public GameObject playerPrefab;
    public CinemachineVirtualCamera cam;
    public static PlayerChecker instance;
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
            player.transform.position = PlayerVars.instance.GetSpawnPos();
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
            PlayerVars.instance.Reset(transform.position);
            PlayerVars.instance.transform.position = PlayerVars.instance.GetSpawnPos();
            PlayerController.instance.virtualCamera = cam;
            // Re-enable the main camera
            cam.gameObject.SetActive(true);
            cam.Follow = PlayerVars.instance.transform;
            PlayerController.instance.levelZoom = cam.m_Lens.OrthographicSize;
        }
    }
}
