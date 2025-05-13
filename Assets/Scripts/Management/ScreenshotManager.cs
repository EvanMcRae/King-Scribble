using UnityEngine;
using System;
using System.IO;

public class ScreenshotManager : MonoBehaviour
{
    public static bool initialized = false;
    private static int numScreenshots;
    public static Action ToggleFullScreen;

    // Use this for initialization
    void Awake()
    {
        if (initialized) return;
        string path = Path.Combine(Application.persistentDataPath, "Screenshots");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        var screenshots = Directory.EnumerateFiles(path);
        foreach (string _ in screenshots)
        {
            numScreenshots++;
        }
        initialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Take screenshots
        if (Input.GetKeyDown(KeyCode.F2))
        {
            ScreenCapture.CaptureScreenshot(Path.Combine(Application.persistentDataPath, "Screenshots", numScreenshots + ".png"));
            numScreenshots++;
        }

        // Toggle full screen
#if UNITY_STANDALONE_OSX
        if (Input.GetKey(KeyCode.LeftCommand) && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F))
        {
            SettingsManager.currentSettings.fullScreen = !SettingsManager.currentSettings.fullScreen;
            Screen.SetResolution(Display.main.systemWidth, (int)(9 / 16f * Display.main.systemWidth), SettingsManager.currentSettings.fullScreen);
            ToggleFullScreen?.Invoke();
        }
#endif

#if UNITY_STANDALONE_WIN
        if (Input.GetKeyDown(KeyCode.F11))
        {
            SettingsManager.currentSettings.fullScreen = !SettingsManager.currentSettings.fullScreen;
            Screen.SetResolution(Display.main.systemWidth, (int)(9 / 16f * Display.main.systemWidth), SettingsManager.currentSettings.fullScreen);
            ToggleFullScreen?.Invoke();
        }
#endif
    }
}
