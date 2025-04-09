using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private GameObject resumeButton;
    [SerializeField] private GameObject SettingsPanel;

    private GameObject previousButton;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.ResetAction += Unpause;
    }

    public void OnDestroy()
    {
        GameManager.ResetAction -= Unpause;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Pause") && PopupPanel.numPopups == 0 && ScreenWipe.over && !GameManager.resetting && !ChangeScene.changingScene)
        {
            if (!GameManager.paused) Pause();
            else Unpause();
        }

        if (GameManager.paused)
        {
            if (EventSystem.current.currentSelectedGameObject == null)
                EventSystem.current.SetSelectedGameObject(previousButton);
            else
                previousButton = EventSystem.current.currentSelectedGameObject;
        }
    }

    public void Pause()
    {
        GameManager.paused = true;
        Time.timeScale = 0;
        foreach (AudioSource _ in FindObjectsOfType<AudioSource>(true))
        {
            _.Pause();
        }
        pauseScreen.SetActive(true);

        if (DrawManager.instance != null)
            DrawManager.instance.SetCursor(ToolType.None);
        EventSystem.current.SetSelectedGameObject(resumeButton);
    }

    public void Unpause()
    {
        GameManager.paused = false;
        Time.timeScale = 1;
        foreach (AudioSource _ in FindObjectsOfType<AudioSource>(true))
        {
            _.UnPause();
        }
        pauseScreen.SetActive(false);

        if (DrawManager.instance != null)
            DrawManager.instance.SetCursor(PlayerVars.instance.cur_tool);
        if (previousButton != null)
        {
            MenuButton prevButton = previousButton.GetComponent<MenuButton>();
            if (prevButton != null)
                prevButton.OnDeselect(null);
        }
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void ResetLevel()
    {
        Unpause();
        GameManager.instance.Reset();
    }

    public void Settings()
    {
        if (!PopupPanel.open)
        {
            SettingsPanel.SetActive(true);
        }
    }

    public void MainMenu()
    {
        Unpause();
        GameManager.resetting = true;
        GameSaver.instance.SaveGame();
        ScreenWipe.instance.WipeIn();
        ScreenWipe.PostWipe += GoToMainMenu;
    }

    public void GoToMainMenu()
    {
        GameManager.resetting = false;
        if (PlayerController.instance != null)
        {
            PlayerController.instance.KillTweens();
            Destroy(PlayerVars.instance.gameObject);
        }
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        Destroy(eventSystem?.gameObject);
        SceneHelper.LoadScene("MainMenu");
        ScreenWipe.PostWipe -= GoToMainMenu;
    }
}
