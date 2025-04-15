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
public class LevelSelectManager : MonoBehaviour
{
    [SerializeField] private Button PlayButton;
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
        if (EventSystem.current.currentSelectedGameObject != null)
            currentSelection = EventSystem.current.currentSelectedGameObject;
        else
            EventSystem.current.SetSelectedGameObject(currentSelection);
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

    public void EnterLevel(string Level)
    {
        if (playing) return;

        // TODO: This won't work for the buttons because it takes variables :(
        //if (!ScreenWipe.over)
        //{
        //    Utils.SetExclusiveAction(ref ScreenWipe.PostUnwipe, PlayGame);
        //    return;
        //}
        //ScreenWipe.PostUnwipe -= PlayGame;

        playing = true;
        ScreenWipe.instance.WipeIn();
        ScreenWipe.PostWipe += () =>
        {
            firstopen = false;
            playing = false;
            PlayerChecker.firstSpawned = false;
            SceneManager.LoadScene(Level);
            Utils.SetExclusiveAction(ref ScreenWipe.PostWipe, null);
        };
    }

    public void MainMenu()
    {
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