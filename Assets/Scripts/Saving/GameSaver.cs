using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameSaver : MonoBehaviour
{
    public SaveSystem saveSystem;
    public static bool loading = false;
    public static GameSaver instance;
    public static PlayerSerialization player;

    public static SaveData currData = SaveData.EmptySave();
    public static List<Sticker.StickerType> tempStickers = new();
    public Dictionary<string, List<string>> unlockPoints = new();

    public static Action StartingSave;
    public static Action<SaveData> loadedNewData;
    public static Action updateDataToSave;

    public void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this);
        Refresh();
        if (PlayerChecker.loadedFromScene)
        {
            WipeScene(currData.scene);
            PlayerChecker.loadedFromScene = false;
        }
    }

    public void WipeSave()
    {
        saveSystem.DeleteSave();
        PlayerVars.lastSavedInventory = new();
        if (PlayerVars.instance != null)
            PlayerVars.instance.inventory = new();
        currData = SaveData.EmptySave();
    }

    public void Refresh()
    {
        string dataToLoad = saveSystem.LoadData();
        if (!string.IsNullOrEmpty(dataToLoad))
        {
            SaveData data = JsonUtility.FromJson<SaveData>(dataToLoad);
            currData = data;
            // Failsafe
            try
            {
                SceneSerialization s = data.scenes.First(s => s.name == data.scene);
            }
            catch (Exception)
            {
                currData.scene = "IntroAnimatic";
                ForceSave();
            }
        }
    }

    public void Clear()
    {
        if (PlayerVars.instance != null)
        {
            Destroy(PlayerVars.instance.gameObject);
            PlayerVars.instance = null;
        }
    }

    public void ForceSave()
    {
        var dataToSave = JsonUtility.ToJson(currData, true);
        saveSystem.SaveData(dataToSave);
    }

    public void SaveGame(bool fromCheckpoint = false) // Sorry Tronster
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

            SceneSerialization s;
            try
            {
                s = data.scenes.First(s => s.name == data.scene);
            }
            catch (Exception)
            {
                s = new SceneSerialization(data.scene, PlayerVars.instance.GetSpawnPos());
                data.scenes.Add(s);
            }

            s.spawnpoint = new Vector3Serialization(PlayerVars.instance.GetSpawnPos());

            if (fromCheckpoint)
            {
                // Specifically serialize ink objects in the current scene
                s.inkPoints.Clear();
                foreach (InkFlood ink in FindObjectsByType<InkFlood>(FindObjectsSortMode.None))
                {
                    s.inkPoints.Add(new InkSerialization(ink));
                }
                try
                {
                    s.unlockPoints = new List<string>(unlockPoints[data.scene]);
                }
                catch (Exception)
                {
                    s.unlockPoints = new();
                }
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
        public List<SceneSerialization> scenes;
        public List<Sticker.StickerType> stickers;

        public void SetPlayer(PlayerVars playerObj)
        {
            player = new PlayerSerialization(playerObj);
        }

        public static SaveData EmptySave()
        {
            SaveData returnData = new();
            returnData.player = new PlayerSerialization();
            returnData.scenes = new();
            returnData.stickers = new();
            returnData.emptySave = true;
            tempStickers = new();
            return returnData;
        }
    }

    // Saves game if player exists on application quit
    private void OnApplicationQuit()
    {
        SaveGame();
    }

    public static SceneSerialization GetScene(string scene)
    {
        try
        {
            SceneSerialization s = currData.scenes.First(s => s.name == scene);
            return s;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static void WipeScene(string scene)
    {
        try
        {
            currData.scenes.First(s => s.name == scene).WipeData();
        }
        catch (Exception)
        {
            return;
        }
    }

    public static void SaveStickers()
    {
        currData.stickers = new(tempStickers);
    }

    public static void ResetStickers()
    {
        tempStickers = new(currData.stickers);
        try
        {
            instance.unlockPoints[currData.scene] = new List<string>(GetScene(currData.scene).unlockPoints);
        }
        catch (Exception)
        {
            instance.unlockPoints[currData.scene] = new List<string>();
        }
    }

    public static void UnlockPoint(string scene, string point)
    {
        try
        {
            instance.unlockPoints[scene].Contains(point);
        }
        catch (Exception)
        {
            try
            {
                instance.unlockPoints[scene] = new List<string>(GetScene(scene).unlockPoints);
            }
            catch (Exception)
            {
                instance.unlockPoints[scene] = new List<string>();
            }
        }
        finally
        {
            if (!instance.unlockPoints[scene].Contains(point))
                instance.unlockPoints[scene].Add(point);
        }
    }
}