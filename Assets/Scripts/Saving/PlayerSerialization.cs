using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerSerialization
{
    public Inventory inventory;
    public ToolType currentTool;

    public PlayerSerialization()
    {
        inventory = new();
        currentTool = ToolType.None;
    }

    public PlayerSerialization(PlayerVars player)
    {
        inventory = PlayerVars.lastSavedInventory;
        currentTool = player.last_tool;
    }

    public void SetValues(GameObject playerObj)
    {
        PlayerVars player = playerObj.GetComponent<PlayerVars>();
        PlayerVars.lastSavedInventory.copy(inventory);
        player.inventory.copy(inventory);
        if (player.inventory.hasTool(currentTool))
            player.cur_tool = currentTool;
    }
}

[Serializable]
public class Vector3Serialization
{
    public float x, y, z;

    public Vector3Serialization(Vector3 position)
    {
        x = position.x;
        y = position.y;
        z = position.z;
    }

    public Vector3 GetValue()
    {
        return new Vector3(x, y, z);
    }
}

[Serializable]
public class SceneSerialization
{
    // TODO extend with other scene data
    public string name;
    public Vector3Serialization spawnpoint;
    public List<string> unlockPoints;
    public List<string> permaUnlockPoints;
    public List<InkSerialization> inkPoints;

    public SceneSerialization(string sceneName, Vector3 spawnPos)
    {
        name = sceneName;
        spawnpoint = new Vector3Serialization(spawnPos);
        inkPoints = new List<InkSerialization>();
    }

    public void WipeData()
    {
        unlockPoints = new();
        inkPoints = new();
    }
}

[Serializable]
public class InkSerialization
{
    public string name;
    public float height;
    public bool flooding;
    public int destination;

    public InkSerialization(InkFlood ink)
    {
        name = ink.gameObject.name;
        height = ink.transform.position.y;
        flooding = ink.flooding;
        destination = ink.curDest;
    }

    public void SetValues(GameObject inkObj)
    {
        InkFlood ink = inkObj.GetComponent<InkFlood>();
        ink.transform.position = new Vector3(inkObj.transform.position.x, height, inkObj.transform.position.z);
        ink.flooding = flooding;
        ink.curDest = destination;
    }
}