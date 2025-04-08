using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static Settings currentSettings = null;
    public const string fileName = "Settings.txt";
    [SerializeField] private Slider qualitySlider;
    [SerializeField] private TextMeshProUGUI qualityValue;
    [SerializeField] private Toggle fullScreenToggle, vSyncToggle;

    void Awake()
    {
        LoadSettings();
        gameObject.SetActive(false);
    }

    public void LoadSettings()
    {
        if (File.Exists(Application.persistentDataPath + "/" + fileName))
            currentSettings = JsonUtility.FromJson<Settings>(File.ReadAllText(Application.persistentDataPath + "/" + fileName));
        currentSettings ??= new Settings();
        UpdateFullScreen(false);
        UpdateVSync(false);
        UpdateQuality(false);
    }

    public static void SaveSettings()
    {
        string path = Application.persistentDataPath + "/" + fileName;
        string settingsJSON = JsonUtility.ToJson(currentSettings, true);

        File.WriteAllText(path, settingsJSON);
        Debug.Log("Saved settings to: " + path);
    }

    public void UpdateQuality(bool user)
    {
        if (user)
            currentSettings.quality = (int)qualitySlider.value;
        else
            qualitySlider.value = currentSettings.quality;

        QualitySettings.SetQualityLevel(currentSettings.quality);
        qualityValue.text = QualitySettings.names[QualitySettings.GetQualityLevel()];
    }

    public void UpdateFullScreen(bool user)
    {
        if (user)
        {
            currentSettings.fullScreen = fullScreenToggle.isOn;
        }
        else
        {
            fullScreenToggle.isOn = currentSettings.fullScreen;
        }
            

        if (currentSettings.fullScreen)
        {
            currentSettings.xRes = (float)3840 / Display.main.systemWidth;
            currentSettings.yRes = (float)2160 / Display.main.systemHeight;
            Screen.SetResolution(3940, 2160, true);
        }
        else
        {
            currentSettings.xRes = Mathf.Clamp(currentSettings.xRes, 0.1f, 1.0f);
            currentSettings.yRes = Mathf.Clamp(currentSettings.yRes, 4 / 9f, 1.0f);
            Screen.SetResolution((int)(currentSettings.xRes * Display.main.systemWidth), (int)(currentSettings.yRes * Display.main.systemHeight), false);
            Screen.SetResolution((int)(currentSettings.xRes * Display.main.systemWidth), (int)(currentSettings.yRes * Display.main.systemHeight), false);
        }
    }

    public void UpdateVSync(bool user)
    {
        if (user)
        {
            currentSettings.vSync = vSyncToggle.isOn;
        }
        else
            vSyncToggle.isOn = currentSettings.vSync;

        if (currentSettings.vSync)
            QualitySettings.vSyncCount = 1;
        else
            QualitySettings.vSyncCount = 0;
    }
}