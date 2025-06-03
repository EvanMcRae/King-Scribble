using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockwaveMan : MonoBehaviour
{
    //how long it takes for shockwave to expand outwards
    //SeraizliedField: adj priv float in inspector
    [SerializeField] private float shockWaveTime = 0.75f;


    private Coroutine shockWaveCoroutine;

    private Material shaderMaterial;

    //shader property to change
    private static int waveDistanceFromCenter = Shader.PropertyToID("waveDistanceFromCenter");

    private void Awake()
    {
        //ref mat on sprite renderer
        shaderMaterial = GetComponent<SpriteRenderer>().material;
    }
            

    public void CallShockwave()
    {
        shockWaveCoroutine = StartCoroutine(ShockWaveAction(-0.1f, 1f));
    }

    private IEnumerator ShockWaveAction(float startPos, float endPos)
    {
        float lerpedAmount = 0f;
        float elapsedTime = 0f;


        while (elapsedTime < shockWaveTime)
        {
            elapsedTime += Time.deltaTime;

            //
            lerpedAmount = Mathf.Lerp(startPos, endPos, (elapsedTime / shockWaveTime));
            //rate at which shockWave expands
            shaderMaterial.SetFloat(waveDistanceFromCenter, lerpedAmount);

            yield return null;
        }
    }

}
