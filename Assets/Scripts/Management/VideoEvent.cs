using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class VideoEvent : MonoBehaviour
{
    [SerializeField] private string scene;
    [SerializeField] private float startAt = 0;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<VideoPlayer>().loopPointReached += EndReached;
        GetComponent<VideoPlayer>().time = startAt;
    }

    void EndReached(VideoPlayer vp)
    {
        GameSaver.currData.scenes.Add(new SceneSerialization(scene, Vector3.zero));
        SceneManager.LoadScene(scene);
    }
}
