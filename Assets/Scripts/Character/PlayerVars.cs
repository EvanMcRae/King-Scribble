using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVars : MonoBehaviour
{
    public static PlayerVars instance;

    public Inventory inventory = null;
    public static Inventory lastSavedInventory = new();
    [SerializeField] private int maxEraserFuel = 500; // RF
    public ToolType cur_tool = ToolType.None;
    public ToolType last_tool = ToolType.None;
    private int curEraserFuel; // RF?
    public bool isDead = false, isResetting = true;
    private Vector3 spawn_pos;
    public bool cheatMode = false;
    public float curCamZoom;

    public void SetSpawnPos(Vector3 spawnPos) {spawn_pos = spawnPos;}
    public Vector3 GetSpawnPos() {return spawn_pos;}
    public float eraserFuelLeft() { return (float)curEraserFuel / maxEraserFuel; }

    public void ReplenishTools()
    {
        foreach (ToolType tool in inventory._toolTypes)
        {
            DrawManager.GetTool(tool).MaxFuel();
        }
    }

    public void RefreshTools()
    {
        foreach (ToolType tool in inventory._toolTypes)
        {
            DrawManager.GetTool(tool).OnStart();
        }
    }

    public void DieEvent()
    {
        Invoke(nameof(Die), 0.75f);
    }

    public void Die()
    {
        GameManager.instance.Reset();
    }

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this);
        spawn_pos = PlayerChecker.instance.transform.position;
    }

    void Start()
    {
        if (inventory == null)
        {
            inventory = new Inventory(); // Initialize a tool inventory containing nothing by default
            lastSavedInventory = new Inventory();
        }
        ReplenishTools();
        last_tool = cur_tool;
    }

    private void Update()
    {
        if (instance == null)
            instance = this;
    }

    public void SaveInventory()
    {
        lastSavedInventory.copy(inventory);
        last_tool = cur_tool;
    }

    // Runs on level reset/death/transition
    public void Reset(Vector3 spawnpoint)
    {
        isResetting = true;
        inventory.copy(lastSavedInventory);
        if (!inventory.hasTool(cur_tool))
            cur_tool = last_tool;

        GetComponentInChildren<SpriteRenderer>().transform.localScale = Vector3.one * (PlayerController.instance.oldPlayer ? 0.7f : 0.15f);
        GetComponent<PlayerController>().facingRight = true;
        GetComponent<PlayerController>().softFall = true;
        transform.position = spawnpoint;
        ReplenishTools();
        GetComponent<PlayerController>().currentSize = PlayerController.SIZE_STAGES;
        GetComponent<PlayerController>().ResizePlayer(1);
        GetComponent<PlayerController>().SetFriction(false);
        GetComponent<PlayerController>().deadLanded = false;
        RefreshTools();
        isDead = false;
        isResetting = false;
    }

    public void Dismount()
    {
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }
}
