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
    private VideoPlayer videoPlayer;

    // Start is called before the first frame update
    void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.url = Path.Combine(Application.streamingAssetsPath, fileName);
        videoPlayer.loopPointReached += EndReached;
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += Play;
    }

    void Play(VideoPlayer vp)
    {
        vp.Play();
        vp.time = startAt;
        ScreenWipe.instance.GetComponent<Animator>().enabled = true;
    }

    void EndReached(VideoPlayer vp)
    {
        GameSaver.currData.scenes.Add(new SceneSerialization(scene, Vector3.zero));
        ScreenWipe.over = false;
        SceneManager.LoadScene(scene);
    }
}
