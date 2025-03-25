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
    public GameObject screenDarkener;
    public const float VOID_DEATH = -100;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Pause") && ScreenWipe.over)
        {
            if (!paused) Pause();
            else Unpause();
        }

        if (Input.GetButtonDown("Reset"))
        {
            if (!resetting && !ChangeScene.changingScene) Reset();
        }

        if (PlayerController.instance.transform.position.y < VOID_DEATH && !resetting)
        {
            Reset();
        }
    }

    public void Pause()
    {
        paused = true;
        Time.timeScale = 0;
        // Stop all sounds
        // Bring up menu
        screenDarkener.SetActive(true);
    }

    public void Unpause()
    {
        paused = false;
        Time.timeScale = 1;
        // Resume all sounds
        // Remove menu
        screenDarkener.SetActive(false);
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
        Unpause();
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        Destroy(eventSystem?.gameObject);
        SceneHelper.LoadScene(SceneManager.GetActiveScene().name);
    }
}
