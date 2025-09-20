using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static Settings currentSettings = null;
    public const string settingsKey = "SettingsData";

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
        if (PlayerPrefs.HasKey(settingsKey))
        {
            string json = PlayerPrefs.GetString(settingsKey);
            currentSettings = JsonUtility.FromJson<Settings>(json);
        }

        currentSettings ??= new Settings();

        // UpdateFullScreen(false);
        UpdateVSync(false);
        UpdateMusic(false);
        UpdateSound(false);
        UpdateMaster(false);
    }

    public static void SaveSettings()
    {
        string settingsJSON = JsonUtility.ToJson(currentSettings, true);
        PlayerPrefs.SetString(settingsKey, settingsJSON);
        PlayerPrefs.Save();
        Debug.Log("Saved settings to PlayerPrefs");
    }

    public void UpdateMusic(bool user)
    {
        if (musicSlider != null)
        {
            if (user)
            {
                currentSettings.musicVolume = musicSlider.value;
            }
            else
            {
                musicSlider.value = currentSettings.musicVolume;
            }
        }

        if (musicSliderFill != null)
        {
            musicSliderFill.fillAmount = musicSlider.value / 100;
        }

        if (musicValue != null)
        {
            musicValue.text = ((int)musicSlider.value).ToString();
        }
    }

    public void UpdateSound(bool user)
    {
        if (soundSlider != null)
        {
            if (user)
            {
                currentSettings.sfxVolume = soundSlider.value;
            }
            else
            {
                soundSlider.value = currentSettings.sfxVolume;
            }
        }

        if (soundSliderFill != null)
        {
            soundSliderFill.fillAmount = soundSlider.value / 100;
        }
        if (soundValue != null)
        {
            soundValue.text = ((int)soundSlider.value).ToString();
        }
    }

    public void UpdateMaster(bool user)
    {
        if (masterSlider != null)
        {
            if (user)
            {
                currentSettings.masterVolume = masterSlider.value;
            }
            else
            {
                masterSlider.value = currentSettings.masterVolume;
            }
        }

        if (masterSliderFill != null)
        {
            masterSliderFill.fillAmount = masterSlider.value / 100;
        }
        if (masterValue != null)
        {
            masterValue.text = ((int)masterSlider.value).ToString();
        }
    }

    public void UpdateFullScreen(bool user)
    {
        if (fullScreenToggle != null)
        {
            if (user)
            {
                currentSettings.fullScreen = fullScreenToggle.isOn;
            }
            else
            {
                fullScreenToggle.isOn = currentSettings.fullScreen;
            }
        }

        Screen.SetResolution(Display.main.systemWidth, (int)(9 / 16f * Display.main.systemWidth), currentSettings.fullScreen);
    }

    public void ToggleFullScreen()
    {
        fullScreenToggle.GetComponent<MenuButton>().noSound = true;
        fullScreenToggle.isOn = currentSettings.fullScreen;
        fullScreenToggle.GetComponent<MenuButton>().noSound = false;
    }

    public void UpdateVSync(bool user)
    {
        if (vSyncToggle != null)
        {
            if (user)
            {
                currentSettings.vSync = vSyncToggle.isOn;
            }
            else
            {
                vSyncToggle.isOn = currentSettings.vSync;
            }
        }

        QualitySettings.vSyncCount = currentSettings.vSync ? 1 : 0;
    }

    // TODO: Temp implementation
    public void DeleteSave()
    {
        GameSaver.instance.WipeSave();
    }
}
