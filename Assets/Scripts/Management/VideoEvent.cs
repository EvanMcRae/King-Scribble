using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class VideoEvent : MonoBehaviour
{
    [SerializeField] private string scene;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<VideoPlayer>().loopPointReached += EndReached;
    }

    void EndReached(VideoPlayer vp)
    {
        GameSaver.currData.unlockedScenes.Add(scene);
        SceneManager.LoadScene(scene);
    }
}
