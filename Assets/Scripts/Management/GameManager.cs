using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Cinemachine;
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{
    public static bool resetting = false, paused = false, canMove = true;
    public static GameManager instance;
    public Transform spawnpoint;
    public float voidDeath = -50;
    public Texture2D defaultCursor, previousCursor;
    public static Action ResetAction;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        // Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {
        if (!paused && Input.GetButtonDown("Reset") && ScreenWipe.over && !resetting && !ChangeScene.changingScene)
        {
            Reset();
        }

        if (PlayerVars.instance.transform.position.y < voidDeath && !resetting)
        {
            Reset();
        }
    }
    
    public void SetCamera(CinemachineVirtualCamera cam)
    {
        cam.gameObject.SetActive(true);
    }

    public void DeactivateCamera(CinemachineVirtualCamera cam)
    {
        cam.gameObject.SetActive(false);
    }
    
    public void SwitchCameras(CinemachineVirtualCamera cam1, CinemachineVirtualCamera cam2, float time)
    {
        StartCoroutine(CameraSwitch(cam1,cam2, time));
    }

    IEnumerator CameraSwitch(CinemachineVirtualCamera cam1, CinemachineVirtualCamera cam2, float time)
    {
        canMove = false;
        // cam1.gameObject.SetActive(false);
        cam2.gameObject.SetActive(true);
        yield return new WaitForSeconds(time);
        cam2.gameObject.SetActive(false);
        // cam1.gameObject.SetActive(true);
        canMove = true;
    }

    public void Reset()
    {
        if (!resetting)
            StartCoroutine(ResetLevel());
    }

    IEnumerator ResetLevel()
    {
        resetting = true;
        ScreenWipe.instance.WipeIn();
        yield return new WaitForSecondsRealtime(1f);
        PlayerVars.instance.Dismount();
        PlayerController.instance.KillTweens();
        ResetAction.Invoke();
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        Destroy(eventSystem?.gameObject);
        Light2D[] Lights = FindObjectsOfType<Light2D>();
        foreach (Light2D light in Lights)
        {
            Destroy(light?.gameObject);
        }
        SceneHelper.LoadScene(SceneManager.GetActiveScene().name);
        canMove = true;
        resetting = false;
    }
}
