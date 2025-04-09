using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEditor;
using TMPro;
using System.IO;

//holds functions of the main menu and sub menus
public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button PlayButton;
    [SerializeField] private PopupPanel InstructionsPanel, SettingsPanel, CreditsPanel;
    private GameObject currentSelection;
    public static bool firstopen = false, quitting = false, playing = false;
    public Texture2D defaultCursor;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.ForceSoftware);

        EventSystem.current.SetSelectedGameObject(PlayButton.gameObject);
        firstopen = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!PopupPanel.open)
        {
            if (EventSystem.current.currentSelectedGameObject != null)
                currentSelection = EventSystem.current.currentSelectedGameObject;
            else
                EventSystem.current.SetSelectedGameObject(currentSelection);
        }
    }

    public void PlayGame()
    {
        if (playing) return;
        if (!ScreenWipe.over)
        {
            Utils.SetExclusiveAction(ref ScreenWipe.PostUnwipe, PlayGame);
            return;
        }
        ScreenWipe.PostUnwipe -= PlayGame;
        SettingsManager.SaveSettings();
        playing = true;
        ScreenWipe.instance.WipeIn();
        ScreenWipe.PostWipe += EnterGame;
    }

    void EnterGame()
    {
        ScreenWipe.PostWipe -= EnterGame;
        firstopen = false;
        playing = false;
        if (SaveSystem.instance.SaveFileExists())
        {
            GameSaver.instance.LoadGame();
        }
        else
        {
            PlayerChecker.firstSpawned = false;
            SceneManager.LoadScene("IntroAnimatic");
        }
    }

    public void Instructions()
    {
        if (!ScreenWipe.over && !playing && !quitting)
        {
            Utils.SetExclusiveAction(ref ScreenWipe.PostUnwipe, Instructions);
            return;
        }
        ScreenWipe.PostUnwipe -= Instructions;
        if (!PopupPanel.open && !playing && !quitting)
        {
            InstructionsPanel.gameObject.SetActive(true);
        }
    }

    public void Settings()
    {
        if (!ScreenWipe.over && !playing && !quitting)
        {
            Utils.SetExclusiveAction(ref ScreenWipe.PostUnwipe, Settings);
            return;
        }
        ScreenWipe.PostUnwipe -= Settings;
        if (ScreenWipe.over && !PopupPanel.open && !playing && !quitting)
        {
            SettingsPanel.gameObject.SetActive(true);
        }
    }

    public void Credits()
    {
        if (!ScreenWipe.over && !playing && !quitting)
        {
            Utils.SetExclusiveAction(ref ScreenWipe.PostUnwipe, Credits);
            return;
        }
        ScreenWipe.PostUnwipe -= Credits;
        if (ScreenWipe.over && !PopupPanel.open && !playing && !quitting)
        {
            CreditsPanel.gameObject.SetActive(true);
        }
    }

    public void Quit()
    {
        if (quitting || playing) return;
        if (!ScreenWipe.over)
        {
            Utils.SetExclusiveAction(ref ScreenWipe.PostUnwipe, Quit);
            return;
        }
        ScreenWipe.PostUnwipe -= Quit;
        quitting = true;
        ScreenWipe.instance.WipeIn();
        ScreenWipe.PostWipe += ExitGame;
    }

    public void ExitGame()
    {
        ScreenWipe.PostWipe -= ExitGame;
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnApplicationQuit()
    {
        SettingsManager.SaveSettings();
    }
}