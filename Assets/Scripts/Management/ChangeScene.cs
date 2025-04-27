using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

public class ChangeScene : MonoBehaviour
{
    public static bool changingScene = false;
    public string scene; // Name of the scene to change to
    public static string nextScene;
    [SerializeField] private SoundPlayer soundPlayer;

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

        // Shrink sequence
        // TODO: A lot of how this is happening is really bad and MUST be changed in the future!! This is for demo sake only.
        soundPlayer.PlaySound("Player.Portal");
        PlayerVars.instance.transform.DOMove(transform.position, 1f);
        PlayerVars.instance.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        PlayerVars.instance.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        Vector3 ogScale = PlayerVars.instance.transform.localScale; // Bandaid fix for scale not resetting otherwise
        PlayerVars.instance.transform.DOScale(Vector3.zero, 1f);
        yield return new WaitForSeconds(1f);
        ScreenWipe.instance.WipeIn();
        yield return new WaitForSeconds(1f);
        PlayerVars.instance.transform.localScale = ogScale;

        PlayerVars.instance.Dismount();
        PlayerController.instance.KillTweens();
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        Destroy(eventSystem?.gameObject);
        Light2D[] Lights = FindObjectsOfType<Light2D>();
        foreach (Light2D light in Lights)
        {
            Destroy(light?.gameObject);
        }
        changingScene = false;
        nextScene = "";
        SceneHelper.LoadScene(scene);
        PlayerVars.instance.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
    }
}
