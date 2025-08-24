using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
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
        GetComponent<CinemachineBrain>().DefaultBlend.Time = blendTime;
    }
}
