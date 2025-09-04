using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem instance;
    public const string saveKey = "SaveData";

    private void Awake()
    {
        instance = this;
    }

    public void SaveData(string dataToSave)
    {
        PlayerPrefs.SetString(saveKey, dataToSave);
        PlayerPrefs.Save();
        Debug.Log("Successfully saved data to PlayerPrefs");
    }

    public string LoadData()
    {
        if (PlayerPrefs.HasKey(saveKey))
        {
            string data = PlayerPrefs.GetString(saveKey);
            Debug.Log("Successfully loaded data from PlayerPrefs");
            return data;
        }
        return "";
    }

    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey(saveKey);
        PlayerPrefs.Save();
        Debug.Log("Successfully deleted save");
    }

    public bool SaveFileExists()
    {
        return PlayerPrefs.HasKey(saveKey);
    }
}
