using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public static bool changingScene = false;
    public string scene;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!changingScene && collision.gameObject.CompareTag("Player") && PlayerController.instance != null && !PlayerController.instance.isDead && !GameManager.resetting) // && !GameSaver.loading)
        {
            StartCoroutine(LoadNextScene());
        }
    }

    IEnumerator LoadNextScene()
    {
        changingScene = true;
        ScreenWipe.instance.WipeIn();
        ScreenWipe.PostUnwipe += () => { changingScene = false; };
        yield return new WaitForSeconds(1f);
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        Destroy(eventSystem?.gameObject);
        SceneHelper.LoadScene(scene);
    }
}
