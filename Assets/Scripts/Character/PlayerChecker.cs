using UnityEngine;
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
            cam.Follow = Instantiate(playerPrefab, transform.position, Quaternion.identity).transform;
        }
        else
        {
            cam.Follow = PlayerVars.instance.transform;
        }
    }
}
