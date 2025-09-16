using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveDataManager : MonoBehaviour
{
    public static Settings currentSettings = null;
    public const string fileName = "Settings.txt";
    [SerializeField] private GameObject arrow, delete1, delete2, delete3;
    [SerializeField] private TextMeshProUGUI percentage1, percentage2, percentage3, stickers1, stickers2, stickers3;
    [SerializeField] private TextMeshProUGUI label1, label2, label3;
    [SerializeField] private GameObject pencil1, pencil2, pencil3;
    [SerializeField] private GameObject pen1, pen2, pen3;
    [SerializeField] private GameObject eraser1, eraser2, eraser3;
    [SerializeField] private GameObject highlighter1, highlighter2, highlighter3;
    [SerializeField] private GameObject deletePanel;
    [SerializeField] private GameObject mainMenuManager;

    private int toDelete = 0; // 0 means it is deselected

    void Awake()
    {
        LoadSaves();
        PopupPanel.numPopups--;
        if (PopupPanel.numPopups < 0) PopupPanel.numPopups = 0;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        
    }

    private void OnDisable()
    {
        
    }

    public void LoadSaves()
    {
        // Fills the GUI with proper data upon enabled!
    }

    public void LoadSave(int save)
    {
        // starts the game with said save slot
        //MainMenuManager.instance.PlayGame();
    }

    public void PromptDeleteSave(int save)
    {
        deletePanel.SetActive(true);
        toDelete = save;
    }

    public void CancelDeleteSave()
    {
        Debug.Log("Keeping Save " + toDelete);
        toDelete = 0;
    }

    public void DeleteSave()
    {
        // delete(toDelete);
        // toDelete is the integer 1-3
        Debug.LogWarning("Deleting Save " + toDelete);
    }

    public void UnlockLevels()
    {
        // Cheat code for unlocking all levels in level select
    }
}