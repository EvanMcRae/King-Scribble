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
    public SoundPlayer soundPlayer;
    public GameObject skipButton;

    public void StartEvent()
    {
        StartCoroutine(Start_Event());

        if (GameSaver.GetScene("Level5") != null && GameSaver.GetScene("Level5").unlockPoints.Contains("cutsceneWatched"))
        {
            skipButton.SetActive(true);
        }
    }

    IEnumerator Start_Event()
    {
        isAnimating = true;
        GameManager.canMove = false;
        yield return new WaitForSeconds(0.5f);
        cam.gameObject.SetActive(true);
        soundPlayer.PlaySound("Ink.Rumble", 1, false);
        var noise = cam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        noise.m_AmplitudeGain = 0.125f;
        DOTween.To(() => noise.m_AmplitudeGain, x => noise.m_AmplitudeGain = x, 0.5f, 4f);
        yield return new WaitForSeconds(4);
        inkFlow_L.transform.DOLocalMoveY(-110, 0.5f);
        inkFlow_R.transform.DOLocalMoveY(-118, 0.5f);
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
        GameSaver.instance.SaveGame();
        GameSaver.GetScene("Level5").unlockPoints.Add("cutsceneWatched");
        skipButton.SetActive(false);
    }

    public void SkipCutscene()
    {
        if (isAnimating)
        {
            StopAllCoroutines();
            soundPlayer.EndAllSounds();
            soundPlayer.PlaySound("Ink.Flood", 1, true);
            cam.gameObject.SetActive(false);
            followCam.Follow = sourceCam.transform;
            inkFlow_L.transform.localPosition = new Vector3(inkFlow_L.transform.localPosition.x, -115f, 0f);
            inkFlow_R.transform.localPosition = new Vector3(inkFlow_R.transform.localPosition.x, -115f, 0f);
            GameManager.canMove = true;
            if (!doorClosed)
            {
                closeDoor.Invoke();
                doorClosed = true;
            }
            startFlood.Invoke();
            isAnimating = false;
            skipButton.SetActive(false);
        }
    }
}
