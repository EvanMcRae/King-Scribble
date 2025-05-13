using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static Settings currentSettings = null;
    public const string fileName = "Settings.txt";
    [SerializeField] private Slider musicSlider, soundSlider, masterSlider;
    [SerializeField] private TextMeshProUGUI musicValue, soundValue, masterValue;
    [SerializeField] private Toggle fullScreenToggle, vSyncToggle;
    [SerializeField] private Image musicSliderFill, soundSliderFill, masterSliderFill;

    void Awake()
    {
        LoadSettings();
        gameObject.SetActive(false);
        ScreenshotManager.ToggleFullScreen += ToggleFullScreen;
    }

    private void OnDestroy()
    {
        ScreenshotManager.ToggleFullScreen -= ToggleFullScreen;
    }

    private void OnDisable()
    {
        SaveSettings();
    }

    public void LoadSettings()
    {
        if (File.Exists(Application.persistentDataPath + "/" + fileName))
            currentSettings = JsonUtility.FromJson<Settings>(File.ReadAllText(Application.persistentDataPath + "/" + fileName));
        currentSettings ??= new Settings();
        UpdateFullScreen(false);
        UpdateVSync(false);
        UpdateMusic(false);
        UpdateSound(false);
        UpdateMaster(false);
    }

    public static void SaveSettings()
    {
        string path = Application.persistentDataPath + "/" + fileName;
        string settingsJSON = JsonUtility.ToJson(currentSettings, true);

        File.WriteAllText(path, settingsJSON);
        Debug.Log("Saved settings to: " + path);
    }

    public void UpdateMusic(bool user)
    {
        if (user)
            currentSettings.musicVolume = musicSlider.value;
        else
            musicSlider.value = currentSettings.musicVolume;

        musicSliderFill.fillAmount = musicSlider.value / 100;
        musicValue.text = (int)musicSlider.value + "";
    }

    public void UpdateSound(bool user)
    {
        if (user)
            currentSettings.sfxVolume = soundSlider.value;
        else
            soundSlider.value = currentSettings.sfxVolume;

        soundSliderFill.fillAmount = soundSlider.value / 100;
        soundValue.text = (int)soundSlider.value + "";
    }

    public void UpdateMaster(bool user)
    {
        if (user)
            currentSettings.masterVolume = masterSlider.value;
        else
            masterSlider.value = currentSettings.masterVolume;

        masterSliderFill.fillAmount = masterSlider.value / 100;
        masterValue.text = (int)masterSlider.value + "";
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
        Screen.SetResolution(Display.main.systemWidth, (int)(9/16f * Display.main.systemWidth), currentSettings.fullScreen);
    }

    public void ToggleFullScreen()
    {
        fullScreenToggle.isOn = currentSettings.fullScreen;
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

    // TODO: Temp implementation
    public void DeleteSave()
    {
        GameSaver.instance.WipeSave();
    }
}