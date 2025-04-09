using System;
using System.IO;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem instance;
    public const string fileName = "SaveData.txt";

    private void Awake()
    {
        instance = this;
    }

    public void SaveData(string dataToSave)
    {
        WriteToFile(dataToSave);
    }

    public string LoadData()
    {
        if (ReadFromFile(out string data))
        {
            Debug.Log("Successfully loaded data");
        }
        return data;
    }

    private static bool WriteToFile(string content)
    {
        var fullPath = Path.Combine(Application.persistentDataPath, fileName);

        try
        {
            File.WriteAllText(fullPath, content);
            Debug.Log("Successfully saved data to " + fullPath);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving to a file " + e.Message);
        }
        return false;
    }

    private bool ReadFromFile(out string content)
    {
        var fullPath = Path.Combine(Application.persistentDataPath, fileName);
        try
        {
            content = File.ReadAllText(fullPath);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Error when loading the file " + e.Message);
            content = "";
        }
        return false;
    }

    public bool SaveFileExists()
    {
        var fullPath = Path.Combine(Application.persistentDataPath, fileName);
        return File.Exists(fullPath);
    }
}