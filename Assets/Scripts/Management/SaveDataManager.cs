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

    void Awake()
    {
        LoadSaves();
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
        // Fills the GUI with proper data
    }

    public void DeleteSave(int save)
    {
        
    }

    public void UnlockLevels()
    {
        // Cheat code for unlocking all levels in level select
    }
}