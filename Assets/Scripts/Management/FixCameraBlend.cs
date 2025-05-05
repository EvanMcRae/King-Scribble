using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class FixCameraBlend : MonoBehaviour
{
    [SerializeField] private float blendTime = 2;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SetCameraBlend());
    }

    IEnumerator SetCameraBlend()
    {
        yield return new WaitForSeconds(0.2f);
        GetComponent<CinemachineBrain>().m_DefaultBlend.m_Time = blendTime;
    }
}
