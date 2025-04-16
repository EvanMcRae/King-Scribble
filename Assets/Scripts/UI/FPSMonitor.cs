using UnityEngine;
using System.Collections;
using TMPro;

// Referenced https://danielkirwan.medium.com/display-fps-on-screen-in-unity-ff5b8946747e
public class FPSMonitor : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI _fpsText;
    [SerializeField] private TextMeshProUGUI _bestfpsText;
    [SerializeField] private TextMeshProUGUI _lowestfpsText;
    [SerializeField] private GameObject menuGroup;
    public float updateInterval = 1.0f;
    private int _bestFps;
    private int _lowestFps;
    private bool active = false;

    private float _currentFPS;

    private void Start()
    {
        _fpsText.text = "FPS: 0";
        _lowestFps = 100;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            active = !active;
            menuGroup.SetActive(active);
        }

        _currentFPS = 1f / Time.unscaledDeltaTime;
        UpdateFPS();
    }

    private void UpdateFPS()
    {
        if (_currentFPS >= _bestFps)
        {
            _bestFps = (int)_currentFPS;
            _bestfpsText.text = $"Best FPS: {_bestFps}";
        }

        if (_lowestFps >= _currentFPS)
        {
            _lowestFps = (int)_currentFPS;
            _lowestfpsText.text = $"Low FPS: {_lowestFps}";
        }

        _fpsText.text = "Curr FPS: " + Mathf.RoundToInt(_currentFPS);
    }
}
