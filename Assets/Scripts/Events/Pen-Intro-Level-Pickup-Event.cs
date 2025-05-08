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

    public void StartEvent()
    {
        StartCoroutine(Start_Event());
    }

    IEnumerator Start_Event()
    {
        isAnimating = true;
        GameManager.canMove = false;
        yield return new WaitForSeconds(0.5f);
        cam.gameObject.SetActive(true);
        yield return new WaitForSeconds(3);
        cam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 1;
        yield return new WaitForSeconds(1);
        inkFlow_L.transform.DOLocalMoveY(-110, 0.5f);
        inkFlow_R.transform.DOLocalMoveY(-118, 0.5f);
        soundPlayer.PlaySound("Ink.Flow", 1, true);
        yield return new WaitForSeconds(3);
        cam.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        closeDoor.Invoke();
        doorClosed = true;
        startFlood.Invoke();
        followCam.Follow = sourceCam.transform;
        GameManager.canMove = true;
        isAnimating = false;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.H) && isAnimating)
        {
            StopAllCoroutines();
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
            if (!soundPlayer.sources[0].isPlaying)
            {
                soundPlayer.PlaySound("Ink.Flow", 1, true);
            }
        }
    }
}
