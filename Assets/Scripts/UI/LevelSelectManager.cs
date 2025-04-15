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

    public string sceneName = "Level1";
    public TextMeshProUGUI levelName, levelDescription;

    public Sprite activeButton;
    public Texture2D defaultCursor;
    public GameObject player;
    public static LevelSelectManager instance;
    public bool goingToMenu = false;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.ForceSoftware);

        // Try to unlock all other buttons
        for (int i = 0; i < buttons.Count; i++)
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
        SelectLevel(index, true);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !goingToMenu && ScreenWipe.over && !playing)
        {
            MainMenu();
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
            player.transform.DOKill();

            if (GameSaver.currData.scene != sceneName)
            {
                // TODO this is a hack to wipe old checkpoint data!!!
                // We should instead keep per-level checkpoint information so you 
                // get sent to the correct spot even if you visit an earlier level.
                // Can't do this yet because URCAD and probably no one will run into this
                GameSaver.currData.quitWhileClearing = true;
                GameSaver.currData.scene = sceneName;
                GameSaver.instance.ForceSave();
            }

            GameSaver.instance.LoadGame();
            Utils.SetExclusiveAction(ref ScreenWipe.PostWipe, null);
        };
    }

    public void MainMenu()
    {
        if (goingToMenu) return;
        goingToMenu = true;
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
        player.transform.DOKill();
        SceneHelper.LoadScene("MainMenu");
        ScreenWipe.PostWipe -= GoToMainMenu;
    }

    public void SelectLevel(int level)
    {
        SelectLevel(level, false);
    }

    public void SelectLevel(int level, bool snap)
    {
        sceneName = sceneNames[level];
        levelName.text = levelNames[level];
        levelDescription.text = "\n" + levelDescriptions[level];

        if (!snap)
            player.transform.DOMove(buttonTransforms[level].position, 0.5f); // TODO temp, want this to follow the line if possible
        else
            player.transform.position = buttonTransforms[level].position;
    }
}