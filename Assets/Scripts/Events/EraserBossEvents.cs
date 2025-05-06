using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;


public class EraserBossEvents : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera cam; // for shaking purposes
    [SerializeField] private GameObject button;
    //public UnityEvent startInkFlow;

    private bool isButtonActive = false;

    // Start is called before the first frame update
    void Start() {
    }

    private void ActivateButton() {
        // change color to purple
        isButtonActive = true;
    }


    public void ButtonPressed() {
        Debug.Log("BUTTON PRESSED");
        if(isButtonActive) {
            Debug.Log("ACTIVATE INK");
        }
        
        
    }

    private IEnumerator PourLeftInkEvent() {
        yield return new WaitForSeconds(1);

        cam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 1;
        yield return new WaitForSeconds(1);
    }
}
