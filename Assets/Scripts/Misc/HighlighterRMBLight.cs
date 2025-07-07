using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Rigidbody2D))]
public class HighlighterRMBLight : MonoBehaviour
{
    [SerializeField] private GameObject _targetPref;
    private Vector2 _targetPos;
    private float _gracePeriod = 0.05f;
    private float _duration = 5f;
    private float _speed = 1f;
    private Rigidbody2D _rigidBody;
    private bool _ignoreCol = false;
    private GameObject _target;
    public void SetTarget(Vector2 targetPos) { _targetPos = targetPos; }
    public void SetDuration(float duration) { _duration = duration; }
    public void SetGrace(float grace) { _gracePeriod = grace; }
    public void SetSpeed(float speed) { _speed = speed; }
    public void StartMove() { Fire(); }
    public delegate void DestroyEvent();
    public DestroyEvent _destroyEvent;

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Highlighter") && !_ignoreCol)
        {
            Break();
        }
    }

    public void Fire()
    {
        StartCoroutine(GracePeriod());
        _target = Instantiate(_targetPref, _targetPos, Quaternion.identity);
        _rigidBody = _rigidBody != null ? _rigidBody : GetComponent<Rigidbody2D>();
        _rigidBody.AddForce((_targetPos - (Vector2)transform.position).normalized * _speed, ForceMode2D.Impulse);
    }

    private IEnumerator GracePeriod() // To prevent immediate destruction due to instant collision with the ground upon instantiation
    {
        _ignoreCol = true;
        float currentTime = 0f;
        while (currentTime < _gracePeriod)
        {
            currentTime += Time.deltaTime;
            yield return null;
        }
        _ignoreCol = false;
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
        Destroy(_target);
    }

    private void Break()
    {
        _destroyEvent?.Invoke();
        Destroy(gameObject);
        if (_target != null) Destroy(_target);
    }

    public void HitTarget()
    {
        _rigidBody.bodyType = RigidbodyType2D.Static;
        gameObject.GetComponent<TrailRenderer>().time = 0.2f;
        StartCoroutine(Fade());
    }
}
