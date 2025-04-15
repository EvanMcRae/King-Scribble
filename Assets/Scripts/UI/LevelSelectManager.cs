using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEditor;
using TMPro;
using System.IO;
using DG.Tweening;

//holds functions of the main menu and sub menus
public class LevelSelectManager : MonoBehaviour
{
    public static bool playing = false;

    public List<GameObject> buttons = new();
    public List<string> sceneNames = new();
    public List<string> levelNames = new();
    public List<string> levelDescriptions = new();
    public List<Transform> buttonTransforms = new();

    public string sceneName = "";
    public TextMeshProUGUI levelName, levelDescription;

    public Sprite activeButton;
    public Texture2D defaultCursor;
    public GameObject player;
    public static LevelSelectManager instance;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.ForceSoftware);

        // Try to unlock all other buttons
        buttons[0].GetComponent<LevelSelectButton>().SetButtonActive(true);
        for (int i = 1; i < buttons.Count; i++)
        {
            int ind = GameSaver.currData.unlockedScenes.IndexOf(sceneNames[i]);
            if (ind != -1)
            {
                buttons[ind].GetComponent<LevelSelectButton>().SetButtonActive(true);
            }
        }

        // Try to select the right scene's button
        sceneName = GameSaver.currData.scene;
        int index = sceneNames.IndexOf(sceneName);
        if (index == -1) index = 0;
        
        EventSystem.current.SetSelectedGameObject(buttons[index]);
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

    public void EnterLevel()
    {
        if (playing) return;
        if (!ScreenWipe.over)
        {
            Utils.SetExclusiveAction(ref ScreenWipe.PostUnwipe, EnterLevel);
            return;
        }
        ScreenWipe.PostUnwipe -= EnterLevel;

        playing = true;
        ScreenWipe.instance.WipeIn();
        ScreenWipe.PostWipe += () =>
        {
            playing = false;
            PlayerChecker.firstSpawned = false;
            SceneManager.LoadScene(sceneName);
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

    public void SelectLevel(int level)
    {
        sceneName = sceneNames[level];
        levelName.text = levelNames[level];
        levelDescription.text = "\n" + levelDescriptions[level];

        // TODO temp, want this to follow the line if possible
        player.transform.DOMove(buttonTransforms[level].position, 0.5f);
    }
}