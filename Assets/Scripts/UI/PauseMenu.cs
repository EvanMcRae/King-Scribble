using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private GameObject resumeButton;
    [SerializeField] private GameObject SettingsPanel;
    [SerializeField] private Image pauseButton;

    private GameObject previousButton;
    public static bool unpausedWithSpace = false;
    public static bool firstopen = false;
    private float prevTimeScale = 1;

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
        unpausedWithSpace = false;

        if (GameManager.paused) firstopen = true;

        if (Input.GetButtonDown("Pause") && PopupPanel.numPopups == 0 && ScreenWipe.over && !GameManager.resetting && !ChangeScene.changingScene)
        {
            if (!GameManager.paused) Pause();
            else Unpause();
        }

        if (GameManager.paused)
        {
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                MenuButton.globalNoSound = true;
                EventSystem.current.SetSelectedGameObject(previousButton);
                MenuButton.globalNoSound = false;
            }
            else
                previousButton = EventSystem.current.currentSelectedGameObject;
        }
    }

    public void Pause()
    {
        if (!(PopupPanel.numPopups == 0 && ScreenWipe.over && !GameManager.resetting && !ChangeScene.changingScene)) return;
        GameManager.paused = true;
        pauseButton.enabled = false;
        prevTimeScale = Time.timeScale;
        Time.timeScale = 0;
        foreach (AudioSource _ in FindObjectsOfType<AudioSource>(true))
        {
            if (!AudioManager.instance.OwnsSource(_))
                _.Pause();
        }
        pauseScreen.SetActive(true);
        AudioManager.instance.PauseEffect(true);

        if (DrawManager.instance != null)
            DrawManager.instance.SetCursor(ToolType.None);
        EventSystem.current.SetSelectedGameObject(resumeButton);
    }

    public void Unpause()
    {
        firstopen = false;
        GameManager.paused = false;
        pauseButton.enabled = true;
        if (Input.GetKeyDown(KeyCode.Space)) unpausedWithSpace = true;
        Time.timeScale = prevTimeScale;
        foreach (AudioSource _ in FindObjectsOfType<AudioSource>(true))
        {
            if (!AudioManager.instance.OwnsSource(_))
                _.UnPause();
        }
        AudioManager.instance.PauseEffect(false);
        pauseScreen.SetActive(false);

        if (DrawManager.instance != null && !HUDButtonCursorHandler.inside && PlayerVars.instance != null)
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
        pauseButton.enabled = false;
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
        pauseButton.enabled = false;
        GameManager.resetting = true;
        GameSaver.instance.SaveGame();
        ScreenWipe.instance.WipeIn();
        ScreenWipe.PostWipe += GoToMainMenu;
    }

    public void GoToMainMenu()
    {
        GameManager.canMove = true;
        if (PlayerController.instance != null)
        {
            PlayerController.instance.KillTweens();
            Destroy(PlayerVars.instance.gameObject);
        }
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        Destroy(eventSystem?.gameObject);
        SceneHelper.LoadScene("MainMenu");
        GameManager.resetting = false;
        ScreenWipe.PostWipe -= GoToMainMenu;
    }
}
