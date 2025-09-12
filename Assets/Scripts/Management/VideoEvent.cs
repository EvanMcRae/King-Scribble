using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class VideoEvent : MonoBehaviour
{
    [SerializeField] private string scene;
    [SerializeField] private float startAt = 0;
    [SerializeField] private string fileName;
    [SerializeField] private List<GameObject> dependencies;
    [SerializeField] private GameObject skipButton;
    private VideoPlayer videoPlayer;

    // Start is called before the first frame update
    void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.url = Path.Combine(Application.streamingAssetsPath, fileName);
        videoPlayer.loopPointReached += EndReached;
        videoPlayer.Prepare();
        if (MainMenuManager.firstopen)
        {
            SceneManager.sceneUnloaded += MenuUnloaded;
        }
        else
        {
            MenuUnloaded(new Scene());
        }
    }

    void Update()
    {
        // TODO: TEMPORARY CHEAT MODE KEYBIND
        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            skipButton.SetActive(true);
        }
    }

    void MenuUnloaded(Scene scene)
    {
        Camera.main.GetComponent<AudioListener>().enabled = true;
        foreach (GameObject dependency in dependencies)
        {
            dependency.SetActive(true);
        }
        SceneManager.sceneUnloaded -= MenuUnloaded;
        if (videoPlayer.isPrepared)
        {
            Play(videoPlayer);
        }
        else
        {
            videoPlayer.prepareCompleted += Play;
        }
    }

    void Play(VideoPlayer vp)
    {
        vp.Play();
        vp.time = startAt;
        ScreenWipe.instance.GetComponent<Animator>().enabled = true;
    }

    void EndReached(VideoPlayer vp)
    {
        LoadScene();
    }

    public void Skip()
    {
        ScreenWipe.instance.WipeIn();
        ScreenWipe.PostWipe += LoadScene;
    }

    void LoadScene()
    {
        ScreenWipe.PostWipe -= LoadScene;
        GameSaver.currData.scenes.Add(new SceneSerialization(scene, Vector3.zero));
        ScreenWipe.over = false;
        SceneManager.LoadScene(scene);
    }
}
