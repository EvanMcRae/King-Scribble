using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class HighlighterRMBLight : MonoBehaviour
{
    private Vector2 _targetPos;
    private float _duration = 5f;
    private float _speed = 1f;
    private bool _moving = false;
    public void SetTarget(Vector2 targetPos) { _targetPos = targetPos; }
    public void SetDuration(float duration) { _duration = duration; }
    public void SetSpeed(float speed) { _speed = speed; }
    public void StartMove() { _moving = true; }
    public delegate void DestroyEvent();
    public DestroyEvent _destroyEvent;

    void FixedUpdate()
    {
        if (_moving)
        {
            gameObject.transform.position = Vector2.MoveTowards(gameObject.transform.position, _targetPos, _speed * Time.fixedDeltaTime);
        }

        if ((Vector2)gameObject.transform.position == _targetPos)
        {
            _moving = false;
            gameObject.GetComponent<TrailRenderer>().time = 0.2f;
            StartCoroutine(Fade());
        }
    }

    private IEnumerator Fade()
    {
        Light2D light = gameObject.GetComponent<Light2D>();
        float startIntensity = light.intensity;
        float currentTime = 0f;
        while (currentTime < _duration)
        {
            currentTime += Time.deltaTime;
            float delta = Mathf.Clamp01(currentTime / _duration);
            light.intensity = Mathf.Lerp(startIntensity, 0, delta);
            yield return null;
        }
        _destroyEvent?.Invoke();
        Destroy(gameObject);
    }
}
