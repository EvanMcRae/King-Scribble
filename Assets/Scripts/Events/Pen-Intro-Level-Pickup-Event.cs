using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class PenIntroLevelPickupEvent : MonoBehaviour
{
    public CinemachineVirtualCamera cam;
    public GameObject inkFlow_L;
    public GameObject inkFlow_R;
    public UnityEvent startFlood;
    public UnityEvent closeDoor;
    public void StartEvent()
    {
        StartCoroutine(Start_Event());
    }

    IEnumerator Start_Event()
    {
        GameManager.canMove = false;
        yield return new WaitForSeconds(0.5f);
        cam.gameObject.SetActive(true);
        yield return new WaitForSeconds(3);
        cam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 1;
        yield return new WaitForSeconds(1);
        inkFlow_L.transform.DOLocalMoveY(-110, 0.5f);
        inkFlow_R.transform.DOLocalMoveY(-110, 0.5f);
        yield return new WaitForSeconds(3);
        cam.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        closeDoor.Invoke();
        startFlood.Invoke();
        GameManager.canMove = true;
    }
}
