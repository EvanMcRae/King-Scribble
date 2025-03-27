using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static bool resetting = false, paused = false;
    public static GameManager instance;
    public Transform spawnpoint;
    public const float VOID_DEATH = -100;
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
        if (Input.GetButtonDown("Reset"))
        {
            if (!resetting && !ChangeScene.changingScene) Reset();
        }

        if (PlayerVars.instance.transform.position.y < VOID_DEATH && !resetting)
        {
            Reset();
        }
    }

    public void Reset()
    {
        StartCoroutine(ResetLevel());
    }

    IEnumerator ResetLevel()
    {
        resetting = true;
        ScreenWipe.instance.WipeIn();
        ScreenWipe.PostUnwipe += () => { resetting = false; };
        yield return new WaitForSecondsRealtime(1f);
        ResetAction.Invoke();
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        Destroy(eventSystem?.gameObject);
        SceneHelper.LoadScene(SceneManager.GetActiveScene().name);
        PlayerVars.instance.Reset(spawnpoint.position);
    }
}
