using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVars : MonoBehaviour
{
    public Inventory inventory;
    [SerializeField] private int maxDoodleFuel = 500;
    [SerializeField] private int maxPenFuel = 1000;
    private int curDoodleFuel;
    private int curPenFuel;
    public delegate void DrawDoodleEvent(float doodlePercent);
    public delegate void DrawPenEvent(float penPercent);
    public DrawDoodleEvent doodleEvent;
    public DrawPenEvent penEvent;
    public int getDoodleFuel() {return curDoodleFuel;}
    public float doodleFuelLeft() {return (float) curDoodleFuel / maxDoodleFuel;}
    public int getPenFuel() {return curPenFuel;}
    public float penFuelLeft() {return (float) curPenFuel / maxPenFuel;}
    public void SpendDoodleFuel(int amount) // Called every time doodle fuel (pencil) is consumed
    {
        curDoodleFuel -= amount;
        if (curDoodleFuel < 0) curDoodleFuel = 0;
        PlayerController.instance.ResizePlayer(doodleFuelLeft());
        if (curDoodleFuel == 0 && !PlayerController.instance.isDead) {
            PlayerController.instance.isDead = true;
            doodleEvent(doodleFuelLeft());
            GameManager.instance.Reset();
        }
        if (!PlayerController.instance.isDead)
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
    void Start()
    {
        inventory = new Inventory(); // Initialize a tool inventory containing only the pencil by default
        curDoodleFuel = maxDoodleFuel;
        curPenFuel = maxPenFuel;
    }
}
