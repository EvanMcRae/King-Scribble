using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockwaveMan : MonoBehaviour
{

    //public GameObject shockPrefab;
    
    //object performing shockwave
    //public GameObject parent;
    //public Transform parTransform;

    //how long it takes for shockwave to expand outwards
    //SeraizliedField: adj priv float in inspector
    [SerializeField] private float shockWaveTime = 2.0f;


    private Coroutine shockWaveCoroutine;

    private Material shaderMaterial;

    //shader property to change
    private static int _waveDistFromCenter = Shader.PropertyToID("_waveDistFromCenter");

    
    private void Awake()
    {
        //ref mat on sprite renderer

        SpriteRenderer shockwaveSprite = GetComponent<SpriteRenderer>();

        //Material shaderMaterial = shockwaveSprite.material;

        shaderMaterial = GetComponent<SpriteRenderer>().material;
    }


    public void CallShockwave()
    {

        /*
        Transform parentTransform = parent.transform;

        //intializes shockwave sprite
        GameObject shockwaveExist = Instantiate(shockPrefab, parentTransform.position,
            parentTransform.rotation, parentTransform);

        Debug.Log("did we spawn?"); */

        /* GameObject shockwaveExist = Instantiate(shockPrefab, this.position,
            this.rotation, this.transform); */

        StopAllCoroutines();

        Debug.Log("did we spawn?");
        Debug.Log($"[{Time.time:F2}] Shockwave animation started for {gameObject.name}. Expected duration: {shockWaveTime}s.", this);
        shockWaveCoroutine = StartCoroutine(ShockWaveAction(-0.1f, 1f));

    }

    private IEnumerator ShockWaveAction(float startPos, float endPos)
    {
        
        shaderMaterial.SetFloat(_waveDistFromCenter, startPos);


        float lerpedAmount = 0f;
        float elapsedTime = 0f;


        while (elapsedTime < shockWaveTime)
        {
            elapsedTime += Time.deltaTime;
            //Debug.Log("this is the elapsedTime " + elapsedTime);


            lerpedAmount = Mathf.Lerp(startPos, endPos, (elapsedTime / shockWaveTime));
            //Debug.Log("this is the lerped amount " + lerpedAmount);
            //rate at which shockWave expands 
            shaderMaterial.SetFloat(_waveDistFromCenter, lerpedAmount);

            yield return null;
        }

        shaderMaterial.SetFloat(_waveDistFromCenter, endPos);
       // Debug.Log($"[{Time.time:F2}] Shockwave animation finished for {gameObject.name} after {elapsedTime:F2}s. Preparing to destroy.", this);
        //destroys sprite once shockwave effect finishes
        Destroy(gameObject);
        //Debug.Log("did we destroy?");

        //Debug.Log($"[{Time.time:F2}] Destroy call completed for {gameObject.name}. (This might not appear if object immediately gone)");
    }

}
