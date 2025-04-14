using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public static bool changingScene = false;
    public string scene; // Name of the scene to change to
    public static string nextScene;

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (!changingScene && collision.gameObject.CompareTag("Player") && PlayerVars.instance != null && !PlayerVars.instance.isDead && !GameManager.resetting) // && !GameSaver.loading)
        {
            StartCoroutine(LoadNextScene());
        }
    }

    public IEnumerator LoadNextScene()
    {
        PlayerVars.instance.SaveInventory();
        changingScene = true;
        nextScene = scene;
        PlayerChecker.firstSpawned = false;
        ScreenWipe.instance.WipeIn();
        yield return new WaitForSeconds(1f);
        PlayerVars.instance.Dismount();
        PlayerController.instance.KillTweens();
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        Destroy(eventSystem?.gameObject);
        changingScene = false;
        nextScene = "";
        SceneHelper.LoadScene(scene);
    }
}
