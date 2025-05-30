using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class PenIntroLevelPickupEvent : MonoBehaviour
{
    public CinemachineVirtualCamera cam, followCam, sourceCam;
    public GameObject inkFlow_L;
    public GameObject inkFlow_R;
    public UnityEvent startFlood;
    public UnityEvent closeDoor;
    private bool doorClosed;
    private bool isAnimating;
    public SoundPlayer rumblePlayer, soundPlayer;
    public GameObject skipButton;
    [SerializeField] Animator anim_L;
    [SerializeField] Animator anim_R;

    public void StartEvent()
    {
        StartCoroutine(Start_Event());

        if (GameSaver.GetScene("Level5") != null && GameSaver.GetScene("Level5").unlockPoints.Contains("cutsceneWatched"))
        {
            skipButton.SetActive(true);
        }
    }

    public void StopEvent()
    {
        // DOTween Ink flows
        inkFlow_L.transform.DOMoveY(-330f, 5f);
        inkFlow_R.transform.DOMoveY(-330f, 5f);
        // Stop anim
        anim_L.Play("Pipe_Stop");
        anim_R.Play("Pipe_Stop");
    }

    IEnumerator Start_Event()
    {
        isAnimating = true;
        GameManager.canMove = false;
        yield return new WaitForSeconds(0.5f);
        cam.gameObject.SetActive(true);
        rumblePlayer.PlaySound("Ink.Rumble", 1, false);
        var noise = cam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        noise.m_AmplitudeGain = 0.125f;
        DOTween.To(() => noise.m_AmplitudeGain, x => noise.m_AmplitudeGain = x, 0.5f, 4f);
        yield return new WaitForSeconds(3);
        anim_L.Play("Pipe_Start");
        anim_R.Play("Pipe_Start");
        yield return new WaitForSeconds(1); // For the Pipe animation to transition from start to flowing
        inkFlow_L.transform.DOLocalMoveY(-118, 2.5f);
        inkFlow_R.transform.DOLocalMoveY(-118, 2.5f);
        soundPlayer.PlaySound("Ink.Flood", 1, true);
        noise.m_AmplitudeGain = 0.25f;
        DOTween.To(() => noise.m_AmplitudeGain, x => noise.m_AmplitudeGain = x, 0f, 3f);
        yield return new WaitForSeconds(3);
        cam.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        closeDoor.Invoke();
        doorClosed = true;
        startFlood.Invoke();
        followCam.Follow = sourceCam.transform;
        GameManager.canMove = true;
        isAnimating = false;
        SceneSerialization s = GameSaver.GetScene("Level5");
        if (s == null)
        {
            s = new("Level5", PlayerVars.instance.GetSpawnPos());
            s.spawnpoint = new Vector3Serialization(PlayerVars.instance.GetSpawnPos());
            s.unlockPoints = new();
            GameSaver.currData.scenes.Add(s);
        }
        s.unlockPoints.Add("cutsceneWatched");
        GameSaver.instance.SaveGame();
        skipButton.GetComponent<HUDButtonCursorHandler>().OnPointerExit(null);
        skipButton.SetActive(false);
    }

    public void SkipCutscene()
    {
        if (isAnimating)
        {
            StopAllCoroutines();
            rumblePlayer.EndAllSounds();
            cam.gameObject.SetActive(false);
            followCam.Follow = sourceCam.transform;
            anim_L.Play("Pipe_Flowing");
            anim_R.Play("Pipe_Flowing");
            inkFlow_L.transform.localPosition = new Vector3(inkFlow_L.transform.localPosition.x, -118f, 0f);
            inkFlow_R.transform.localPosition = new Vector3(inkFlow_R.transform.localPosition.x, -118f, 0f);
            GameManager.canMove = true;
            if (!doorClosed)
            {
                closeDoor.Invoke();
                doorClosed = true;
            }
            startFlood.Invoke();
            isAnimating = false;
            skipButton.GetComponent<HUDButtonCursorHandler>().OnPointerExit(null);
            skipButton.SetActive(false);
        }
    }
}
