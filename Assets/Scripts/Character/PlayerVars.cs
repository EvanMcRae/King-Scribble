using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVars : MonoBehaviour
{
    public static PlayerVars instance;

    public Inventory inventory, lastSavedInventory;
    [SerializeField] private int maxDoodleFuel = 500;
    [SerializeField] private int maxPenFuel = 1000;
    public ToolType cur_tool = ToolType.None;
    private int curDoodleFuel;
    private int curPenFuel;
    public delegate void DrawDoodleEvent(float doodlePercent);
    public delegate void DrawPenEvent(float penPercent);
    public DrawDoodleEvent doodleEvent;
    public DrawPenEvent penEvent;
    public bool isDead = false;
    public int getDoodleFuel() {return curDoodleFuel;}
    public float doodleFuelLeft() {return (float) curDoodleFuel / maxDoodleFuel;}
    public int getPenFuel() {return curPenFuel;}
    public float penFuelLeft() {return (float) curPenFuel / maxPenFuel;}

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
        penEvent(penFuelLeft());
    }
    public void AddDoodleFuel(int amount) {
        curDoodleFuel += amount;
        if (curDoodleFuel > maxDoodleFuel) curDoodleFuel = maxDoodleFuel; // shouldn't happen but just in case
        PlayerController.instance.ResizePlayer(doodleFuelLeft());
        doodleEvent(doodleFuelLeft());
    }

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        inventory = new Inventory(); // Initialize a tool inventory containing nothing by default
        lastSavedInventory = new Inventory();
        curDoodleFuel = maxDoodleFuel;
        curPenFuel = maxPenFuel;
    }

    private void Update()
    {
        if (instance == null)
            instance = this;
    }

    // Runs on level reset/death/transition
    public void Reset(Vector3 spawnpoint)
    {
        cur_tool = ToolType.None;
        inventory = lastSavedInventory;

        GetComponent<PlayerController>().facingRight = false;
        transform.position = spawnpoint;
        curDoodleFuel = maxDoodleFuel;
        curPenFuel = maxPenFuel;
        isDead = false;
        GetComponent<PlayerController>().ResizePlayer(doodleFuelLeft());
    }
}
