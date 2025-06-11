using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVars : MonoBehaviour
{
    public static PlayerVars instance;

    public Inventory inventory = null;
    public static Inventory lastSavedInventory = new();
    [SerializeField] private int maxDoodleFuel = 750; // RF
    [SerializeField] private int maxPenFuel = 1000; // RF
    [SerializeField] private int maxEraserFuel = 500; // RF
    public Pencil _pencil;
    public Pen _pen;
    public Eraser _eraser;
    public ToolType cur_tool = ToolType.None;
    private int curDoodleFuel; // RF?
    private int curPenFuel, tempPenFuel; // RF?
    private int curEraserFuel; // RF?
    public delegate void DrawDoodleEvent(float doodlePercent); // RF?
    public delegate void DrawPenEvent(float penPercent); // RF?
    public delegate void PenMonitorEvent(float penPercent); // RF?
    public delegate void EraseEvent(float erasePercent); // RF?
    public DrawDoodleEvent doodleEvent; // RF?
    public DrawPenEvent penEvent; // RF?
    public PenMonitorEvent penMonitorEvent; // RF?
    public EraseEvent eraseEvent; // RF?
    public Action releaseEraser; // RF?
    public bool isDead = false;
    private Vector3 spawn_pos;
    public bool cheatMode = false;
    public float curCamZoom;

    public void SetSpawnPos(Vector3 spawnPos) {spawn_pos = spawnPos;}
    public Vector3 GetSpawnPos() {return spawn_pos;}
    public int getDoodleFuel() {return curDoodleFuel;}
    public float doodleFuelLeft() {return (float) curDoodleFuel / maxDoodleFuel;}
    public int getPenFuel() {return curPenFuel;}
    public float penFuelLeft() {return (float) curPenFuel / maxPenFuel;}
    public float tempPenFuelLeft() { return (float)tempPenFuel / maxPenFuel; }
    public int getEraserFuel() { return curEraserFuel; }
    public float eraserFuelLeft() { return (float)curEraserFuel / maxEraserFuel; }

    public void SpendDoodleFuel(int amount) // Called every time doodle fuel (pencil) is consumed
    {
        if (cheatMode) return;
        curDoodleFuel -= amount;
        if (curDoodleFuel < 0) curDoodleFuel = 0;
        PlayerController.instance.ResizePlayer(doodleFuelLeft());
        if (curDoodleFuel == 0 && !isDead) {
            isDead = true;
            PlayerController.instance.DeathSound();
            doodleEvent(doodleFuelLeft());
            Invoke(nameof(Die), 0.75f);
        }
        if (!isDead)
            doodleEvent(doodleFuelLeft());
    }

    public void Die()
    {
        GameManager.instance.Reset();
    }

    public void SpendPenFuel(int amount) // Called every time pen fuel (pen - obviously) is consumed
    {
        if (cheatMode) return;
        curPenFuel -= amount;
        if (curPenFuel < 0) curPenFuel = 0;
        tempPenFuel = curPenFuel;
        penEvent(penFuelLeft());
    }
    public void SpendTempPenFuel(int amount) // Called while pen is drawing to monitor maximum draw amount
    {
        if (cheatMode) return;
        tempPenFuel -= amount;
        if (tempPenFuel < 0) tempPenFuel = 0;
        penMonitorEvent((float)tempPenFuel / maxPenFuel);
    }
    public void ResetTempPenFuel() // Called if the pen fails to draw a physics object
    {
        if (cheatMode) return;
        tempPenFuel = curPenFuel;
        penMonitorEvent((float)tempPenFuel / maxPenFuel);
    }

    public void SpendEraserFuel(int amount) // Called every time eraser fuel is consumed
    {
        if (cheatMode) return;
        curEraserFuel -= amount;
        if (curEraserFuel < 0) curEraserFuel = 0;
        eraseEvent(eraserFuelLeft());
    }

    public void ReplenishEraser()
    {
        curEraserFuel = maxEraserFuel;
        eraseEvent(1);
    }

    public void AddDoodleFuel(int amount) {
        if (cheatMode) return;
        curDoodleFuel += amount;
        if (curDoodleFuel > maxDoodleFuel) curDoodleFuel = maxDoodleFuel; // shouldn't happen but just in case
        PlayerController.instance.ResizePlayer(doodleFuelLeft());
        doodleEvent(doodleFuelLeft());
    }

    public void AddPenFuel(int amount) {
        if (cheatMode) return;
        curPenFuel += amount;
        tempPenFuel += amount;
        if (curPenFuel > maxPenFuel) curPenFuel = maxPenFuel;
        if (tempPenFuel > maxPenFuel) tempPenFuel = maxPenFuel;
        penEvent(penFuelLeft());
        penMonitorEvent((float)tempPenFuel / maxPenFuel);
    }
    
    public void MaxDoodleFuel() {
        curDoodleFuel = maxDoodleFuel;
        PlayerController.instance.ResizePlayer(doodleFuelLeft());
        doodleEvent(doodleFuelLeft());
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
        curDoodleFuel = maxDoodleFuel;
        curPenFuel = maxPenFuel;
        tempPenFuel = maxPenFuel;
        curEraserFuel = maxEraserFuel;
    }

    private void Update()
    {
        if (instance == null)
            instance = this;
    }

    public void SaveInventory()
    {
        lastSavedInventory.copy(inventory);
    }

    // Runs on level reset/death/transition
    public void Reset(Vector3 spawnpoint)
    {
        inventory.copy(lastSavedInventory);
        if (!inventory.hasTool(cur_tool))
            cur_tool = ToolType.None;

        GetComponentInChildren<SpriteRenderer>().transform.localScale = Vector3.one * (PlayerController.instance.oldPlayer ? 0.7f : 0.15f);
        GetComponent<PlayerController>().facingRight = true;
        GetComponent<PlayerController>().softFall = true;
        transform.position = spawnpoint;
        curDoodleFuel = maxDoodleFuel;
        curPenFuel = maxPenFuel;
        tempPenFuel = maxPenFuel;
        curEraserFuel = maxEraserFuel;
        isDead = false;
        GetComponent<PlayerController>().currentSize = PlayerController.SIZE_STAGES;
        GetComponent<PlayerController>().ResizePlayer(doodleFuelLeft());
        GetComponent<PlayerController>().SetFriction(false);
        GetComponent<PlayerController>().deadLanded = false;
    }

    public void Dismount()
    {
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }
}
