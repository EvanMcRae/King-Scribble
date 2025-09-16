using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using UnityEngine.Splines;

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
    public bool firstOpen = false;
    public int currLevel;
    public Tween currTween;
    public bool fadeOutMusic = false;

    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private int sampleCount = 50;

    [SerializeField] private SoundPlayer soundPlayer;
    [SerializeField] private GameObject skipButton;

    // Start is called before the first frame update
    void Start()
    {
        //Time.timeScale = 0.02f;
        instance = this;

        Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);

        // Try to unlock all other buttons
        for (int i = 0; i < buttons.Count; i++)
        {
            if (GameSaver.GetScene(sceneNames[i]) != null)
            {
                buttons[i].GetComponent<LevelSelectButton>().SetButtonActive(true);
            }
        }

        // Try to select the right scene's button
        GameSaver.instance.Refresh();
        sceneName = GameSaver.currData.scene;
        int index = sceneNames.IndexOf(sceneName);
        if (index == -1) index = 0;
        // This lowkey sucks mb
        buttons[0].GetComponent<LevelSelectButton>().SetButtonActive(true);
        EventSystem.current.SetSelectedGameObject(buttons[index]);
        SelectLevel(index, true);

        firstOpen = true;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !goingToMenu && ScreenWipe.over && !playing)
        {
            MainMenu();
        }

        // TODO: DEBUG KEY
        if (Utils.CHEATS_ENABLED && Input.GetKeyDown(KeyCode.Backslash))
        {
            skipButton.SetActive(true);
        }
    }

    public void UnlockAll()
    {
        // Try to unlock all other buttons
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].GetComponent<LevelSelectButton>().SetButtonActive(true);
        }
    }

    public void LateUpdate()
    {
        UpdateYPos();
    }

    public void UpdateYPos()
    {
        Vector3 pos = player.transform.position;
        pos.y = splineContainer.EvaluatePosition(GetTFromX(player.transform.position.x)).y;
        Vector3 vel = Vector3.zero;
        Vector3 smoothPos = Vector3.SmoothDamp(player.transform.position, pos, ref vel, 0.015f);
        pos.y = smoothPos.y;
        player.transform.position = pos;
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

        Debug.Log(currLevel);
        if (currLevel >= 4) // TODO: remove, this is a hack :(
        {
            AudioManager.instance.carryOn = false;
            AudioManager.instance.FadeOutCurrent(0.5f);
        }

        ScreenWipe.instance.WipeIn();
        ScreenWipe.PostWipe += () =>
        {
            playing = false;
            PlayerChecker.firstSpawned = false;
            player.transform.DOKill();

            if (GameSaver.currData.scene != sceneName)
            {
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
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
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
        {
            currTween = player.transform.DOMoveX(buttonTransforms[level].position.x, Mathf.Abs(GetTFromX(player.transform.position.x) - GetTFromX(buttonTransforms[level].position.x)) * 1.5f).SetEase(Ease.Linear);
            if (firstOpen && !buttons[level].GetComponent<LevelSelectButton>().playedSound)
            {
                soundPlayer.PlaySound("UI.Press");
                buttons[level].GetComponent<LevelSelectButton>().playedSound = true;
            }
        }
        else
            player.transform.position = buttonTransforms[level].position;

        currLevel = level;
    }

    float GetTFromX(float targetX)
    {
        float bestT = 0;
        float minDistance = float.MaxValue;
        for (int i = 0; i <= sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            Vector3 point = splineContainer.EvaluatePosition(t);
            float distance = Mathf.Abs(point.x - targetX);

            if (distance < minDistance)
            {
                minDistance = distance;
                bestT = t;
            }
        }

        return bestT;
    }
}