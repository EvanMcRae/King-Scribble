using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{
    public static bool resetting = false, paused = false, canMove = true;
    public static GameManager instance;
    public Transform spawnpoint;
    public float voidDeath = -50;
    public Texture2D defaultCursor, previousCursor;
    public static Action ResetAction;
    public ResetPrompt resetPrompt;
    public float resetTime;
    public const float maxResetTime = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        //Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {
        // Held down reset control
        if (!paused && ScreenWipe.over && !resetting && !ChangeScene.changingScene)
        {
            if (Input.GetButtonDown("Reset"))
            {
                resetPrompt.SetVisibility(true);
                resetTime = 0;
            }

            if (Input.GetButton("Reset"))
            {
                resetPrompt.SetVisibility(true);
                if (resetTime >= maxResetTime)
                {
                    ResetGame();
                }
                else
                {
                    resetPrompt.SetFill(resetTime / maxResetTime);
                    resetTime += Time.deltaTime;
                }
            }

            if (Input.GetButtonUp("Reset"))
            {
                ClearResetPrompt();
            }
        }

        if (paused)
        {
            ClearResetPrompt();
        }

        if (PlayerVars.instance.transform.position.y < voidDeath && !resetting)
        {
            ResetGame();
        }
    }

    void ClearResetPrompt()
    {
        resetPrompt.SetVisibility(false);
        resetTime = 0;
        resetPrompt.SetFill(resetTime);
    }

    public void SetCamera(CinemachineCamera cam)
    {
        cam.gameObject.SetActive(true);
        PlayerVars.instance.curCamZoom = cam.Lens.OrthographicSize;
    }

    public void DeactivateCamera(CinemachineCamera cam)
    {
        cam.gameObject.SetActive(false);
    }
    
    public void SwitchCameras(CinemachineCamera cam1, CinemachineCamera cam2, float time)
    {
        StartCoroutine(CameraSwitch(cam1,cam2, time));
    }

    IEnumerator CameraSwitch(CinemachineCamera cam1, CinemachineCamera cam2, float time)
    {
        canMove = false;
        // cam1.gameObject.SetActive(false);
        cam2.gameObject.SetActive(true);
        PlayerVars.instance.curCamZoom = cam2.Lens.OrthographicSize;
        yield return new WaitForSeconds(time);
        cam2.gameObject.SetActive(false);
        PlayerVars.instance.curCamZoom = cam1.Lens.OrthographicSize;
        // cam1.gameObject.SetActive(true);
        canMove = true;
    }

    public void ResetGame()
    {
        if (!resetting)
            StartCoroutine(ResetLevel());
    }

    IEnumerator ResetLevel()
    {
        resetting = true;
        ClearResetPrompt();
        ScreenWipe.instance.WipeIn();
        GameSaver.ResetStickers();
        ResetAction.Invoke();
        yield return new WaitForSeconds(1f);
        PlayerVars.instance.Dismount();
        PlayerController.instance.KillTweens();
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
        Destroy(eventSystem?.gameObject);
        Light2D[] Lights = FindObjectsByType<Light2D>(FindObjectsSortMode.None);
        foreach (Light2D light in Lights)
        {
            Destroy(light?.gameObject);
        }
        SceneHelper.LoadScene(SceneManager.GetActiveScene().name);
        canMove = true;
        resetting = false;
    }
}
