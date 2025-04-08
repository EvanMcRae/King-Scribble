using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVars : MonoBehaviour
{
    public static PlayerVars instance;

    public Inventory inventory, lastSavedInventory;
    [SerializeField] private int maxDoodleFuel = 750;
    [SerializeField] private int maxPenFuel = 1000;
    [SerializeField] private int maxEraserFuel = 500;
    public ToolType cur_tool = ToolType.None;
    private int curDoodleFuel;
    private int curPenFuel, tempPenFuel;
    private int curEraserFuel;
    public delegate void DrawDoodleEvent(float doodlePercent);
    public delegate void DrawPenEvent(float penPercent);
    public delegate void PenMonitorEvent(float penPercent);
    public delegate void EraseEvent(float erasePercent);
    public DrawDoodleEvent doodleEvent;
    public DrawPenEvent penEvent;
    public PenMonitorEvent penMonitorEvent;
    public EraseEvent eraseEvent;
    public Action releaseEraser;
    public bool isDead = false;
    private Vector3 spawn_pos;
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
        curDoodleFuel -= amount;
        if (curDoodleFuel < 0) curDoodleFuel = 0;
        PlayerController.instance.ResizePlayer(doodleFuelLeft());
        if (curDoodleFuel == 0 && !isDead) {
            isDead = true;
            doodleEvent(doodleFuelLeft());
            GameManager.instance.Reset();
        }
        if (!isDead)
            doodleEvent(doodleFuelLeft());
    }

    public void SpendPenFuel(int amount) // Called every time pen fuel (pen - obviously) is consumed
    {
        curPenFuel -= amount;
        if (curPenFuel < 0) curPenFuel = 0;
        tempPenFuel = curPenFuel;
        penEvent(penFuelLeft());
    }
    public void SpendTempPenFuel(int amount) // Called while pen is drawing to monitor maximum draw amount
    {
        tempPenFuel -= amount;
        if (tempPenFuel < 0) tempPenFuel = 0;
        penMonitorEvent((float)tempPenFuel / maxPenFuel);
    }
    public void ResetTempPenFuel() // Called if the pen fails to draw a physics object
    {
        tempPenFuel = curPenFuel;
        penMonitorEvent((float)tempPenFuel / maxPenFuel);
    }

    public void SpendEraserFuel(int amount) // Called every time eraser fuel is consumed
    {
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
        curDoodleFuel += amount;
        if (curDoodleFuel > maxDoodleFuel) curDoodleFuel = maxDoodleFuel; // shouldn't happen but just in case
        PlayerController.instance.ResizePlayer(doodleFuelLeft());
        doodleEvent(doodleFuelLeft());
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
        inventory = new Inventory(); // Initialize a tool inventory containing nothing by default
        lastSavedInventory = new Inventory();
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

        GetComponent<PlayerController>().facingRight = true;
        transform.position = spawnpoint;
        curDoodleFuel = maxDoodleFuel;
        curPenFuel = maxPenFuel;
        isDead = false;
        GetComponent<PlayerController>().ResizePlayer(doodleFuelLeft());
    }

    public void Dismount()
    {
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }
}
