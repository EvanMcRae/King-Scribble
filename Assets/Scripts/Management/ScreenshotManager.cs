using UnityEngine;
using System.IO;

public class ScreenshotManager : MonoBehaviour
{
    private static bool initialized = false;
    private static int numScreenshots;

    // Use this for initialization
    void Awake()
    {
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
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            ScreenCapture.CaptureScreenshot(Path.Combine(Application.persistentDataPath, "Screenshots", numScreenshots + ".png"));
            numScreenshots++;
        }

    }
}
