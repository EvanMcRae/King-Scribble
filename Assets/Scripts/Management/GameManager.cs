using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Cinemachine;
public class GameManager : MonoBehaviour
{
    public static bool resetting = false, paused = false, canMove = true;
    public static GameManager instance;
    public Transform spawnpoint;
    public const float VOID_DEATH = -50;
    public Texture2D defaultCursor, previousCursor;
    public static Action ResetAction;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (!paused && Input.GetButtonDown("Reset"))
        {
            if (!resetting && !ChangeScene.changingScene) Reset();
        }

        if (PlayerVars.instance.transform.position.y < VOID_DEATH && !resetting)
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
        ScreenWipe.PostUnwipe += () => { resetting = false; };
        yield return new WaitForSecondsRealtime(1f);
        PlayerVars.instance.Dismount();
        PlayerController.instance.KillTweens();
        ResetAction.Invoke();
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        Destroy(eventSystem?.gameObject);
        SceneHelper.LoadScene(SceneManager.GetActiveScene().name);
        canMove = true;
    }
}
