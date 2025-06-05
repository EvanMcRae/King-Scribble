//used for the actual shockwave animation over set rate of time
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockwaveMan : MonoBehaviour
{

    //how long it takes for shockwave to expand outwards
    [SerializeField] private float shockWaveTime = 2.0f;
    private Coroutine shockWaveCoroutine;
    private Material shaderMaterial;

    //shader property to change (how the wave spreads)
    private static int _waveDistFromCenter = Shader.PropertyToID("_waveDistFromCenter");
    
    private void Awake()
    {
        //refercing sprite material used for animation
        SpriteRenderer shockwaveSprite = GetComponent<SpriteRenderer>();
        if (shockwaveSprite != null)
        {
            shaderMaterial = GetComponent<SpriteRenderer>().material;
        }
    }

    //queues the shockwave animation
    public void CallShockwave()
    {
        StopAllCoroutines();
        shockWaveCoroutine = StartCoroutine(ShockWaveAction(-0.1f, 1f));

    }

    private IEnumerator ShockWaveAction(float startPos, float endPos)
    {
        if (shaderMaterial != null)
        {
            shaderMaterial.SetFloat(_waveDistFromCenter, startPos);

            float lerpedAmount = 0f;
            float elapsedTime = 0f;

            while (elapsedTime < shockWaveTime)
            {
                elapsedTime += Time.deltaTime;

                //used for smoothing the effect
                lerpedAmount = Mathf.Lerp(startPos, endPos, (elapsedTime / shockWaveTime));
                //rate at which shockWave expands 
                shaderMaterial.SetFloat(_waveDistFromCenter, lerpedAmount);

                yield return null;
            }

            //just to make ensure the shockwave has finshed
            shaderMaterial.SetFloat(_waveDistFromCenter, endPos);
            //destroys sprite once shockwave effect finishes
            Destroy(gameObject);
        }
    }
}
