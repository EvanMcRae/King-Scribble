using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

public class SwitchLight : MonoBehaviour
{
    [SerializeField] private Light2D _light;
    [SerializeField] private float _initialIntensity;
    [SerializeField] private float _finalIntensity;
    [Tooltip("A duration of 0 will cause the light to remain at full intensity permanently once activated.")]
    [SerializeField] private float _duration;
    public UnityEvent _onActivate;

    void Start()
    {
        _light.intensity = _initialIntensity;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("HighlighterRMB"))
        {
            _onActivate?.Invoke();
            _light.intensity = _finalIntensity;
            col.gameObject.GetComponent<HighlighterRMBLight>()._destroyEvent?.Invoke();
            Destroy(col.gameObject);
            if (_duration != 0)
                StartCoroutine(Fade());
        }
    }

    private IEnumerator Fade()
    {
        float currentTime = 0f;
        while (currentTime < _duration)
        {
            currentTime += Time.deltaTime;
            float delta = Mathf.Clamp01(currentTime / _duration);
            _light.intensity = Mathf.Lerp(_finalIntensity, _initialIntensity, delta);
            yield return null;
        }
    }
}
