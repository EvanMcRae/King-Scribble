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
    private static int _waveDistFromCenter = Shader.PropertyToID("_waveDistFromCenter");

    private void Awake()
    {
        //ref mat on sprite renderer
        shaderMaterial = GetComponent<SpriteRenderer>().material;
    }
            

    public void CallShockwave()
    {
        shockWaveCoroutine = StartCoroutine(ShockWaveAction(-0.1f, 30f));
    }

    private IEnumerator ShockWaveAction(float startPos, float endPos)
    {
        
        shaderMaterial.SetFloat(_waveDistFromCenter, startPos);


        float lerpedAmount = 0f;
        float elapsedTime = 0f;


        while (elapsedTime < shockWaveTime)
        {
            elapsedTime += Time.deltaTime;
            Debug.Log("this is the elapsedTime " + elapsedTime);


            lerpedAmount = Mathf.Lerp(startPos, endPos, (elapsedTime / shockWaveTime));
            Debug.Log("this is the lerped amount " + lerpedAmount);
            //rate at which shockWave expands 
            shaderMaterial.SetFloat(_waveDistFromCenter, lerpedAmount);

            yield return null;
        }
    }

}
