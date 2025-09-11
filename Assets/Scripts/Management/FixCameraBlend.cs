using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineBrain))]
public class FixCameraBlend : MonoBehaviour
{
    private static readonly WaitForSeconds _waitForSeconds0_2 = new(0.2f);
    [SerializeField] private float blendTime = 2;

    // Start is called before the first frame update
    void Start()
    {
        FixBlend();
    }

    public void FixBlend()
    {
        StartCoroutine(SetCameraBlend());
    }

    IEnumerator SetCameraBlend()
    {
        GetComponent<CinemachineBrain>().DefaultBlend.Time = 0;
        yield return _waitForSeconds0_2;
        GetComponent<CinemachineBrain>().DefaultBlend.Time = blendTime;
    }
}
