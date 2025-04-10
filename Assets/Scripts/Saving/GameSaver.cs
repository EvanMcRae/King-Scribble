using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameSaver : MonoBehaviour
{
    public SaveSystem saveSystem;
    public static bool loading = false;
    public static GameSaver instance;
    public static PlayerSerialization player;

    public static SaveData currData = SaveData.EmptySave();

    public static Action StartingSave;
    public static Action<SaveData> loadedNewData;
    public static Action updateDataToSave;

    public void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this);
    }

    public void Clear()
    {
        if (PlayerVars.instance != null)
        {
            Destroy(PlayerVars.instance.gameObject);
            PlayerVars.instance = null;
        }
    }

    public void SaveGame()
    {
        if (!loading && PlayerVars.instance != null)
        {
            StartingSave?.Invoke();
            SaveData data = currData;
            data.SetPlayer(PlayerVars.instance);
            data.emptySave = false;
            if (ChangeScene.changingScene)
            {
                data.scene = ChangeScene.nextScene;
                data.quitWhileClearing = true;
            }
            else
            {
                data.scene = SceneManager.GetActiveScene().name;
                data.quitWhileClearing = false;
            }
            
            var dataToSave = JsonUtility.ToJson(data, true);
            saveSystem.SaveData(dataToSave);
        }
    }

    public void LoadGame()
    {
        // Load game can only happen from menu
        if (!loading && PlayerVars.instance == null)
        {
            loading = true;
            string dataToLoad = saveSystem.LoadData();

            if (!string.IsNullOrEmpty(dataToLoad))
            {
                Clear();
                SaveData data = JsonUtility.FromJson<SaveData>(dataToLoad);
                player = data.player;
                currData = data;
                EventSystem eventSystem = FindObjectOfType<EventSystem>();
                Destroy(eventSystem?.gameObject);
                PlayerChecker.firstSpawned = false;
                SceneHelper.LoadScene(data.scene);
                loadedNewData?.Invoke(data);
            }
            else
                loading = false;
        }
    }

    [Serializable]
    public class SaveData
    {
        public bool emptySave = false, quitWhileClearing = false;
        public PlayerSerialization player;
        public string scene = "IntroAnimatic";
        // TODO add unlocked levels to this

        public void SetPlayer(PlayerVars playerObj)
        {
            player = new PlayerSerialization(playerObj);
        }

        public static SaveData EmptySave()
        {
            SaveData returnData = new();
            returnData.emptySave = true;
            return returnData;
        }
    }

    // Saves game if player exists on application quit
    private void OnApplicationQuit()
    {
        SaveGame();
    }
}