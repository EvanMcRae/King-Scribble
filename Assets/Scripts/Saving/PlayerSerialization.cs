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

    public void WipeData(bool includePerma)
    {
        unlockPoints = new();
        inkPoints = new();
        if (includePerma)
            permaUnlockPoints = new();
    }
}

[Serializable]
public class InkSerialization
{
    public string name;
    public float height;
    public bool flooding;
    public int destination;
    public bool couldSoftlock;

    public InkSerialization(InkFlood ink)
    {
        name = ink.gameObject.name;
        height = ink.transform.position.y;
        flooding = ink.flooding;
        destination = ink.curDest;
        couldSoftlock = ink.couldSoftlock;
    }

    public void SetValues(GameObject inkObj)
    {
        InkFlood ink = inkObj.GetComponent<InkFlood>();

        // Make sure that the ink doesn't load too close to the player, or else you can softlock yourself
        float bound = height;
        if (flooding && couldSoftlock)
        {
            bound = GameSaver.GetScene(GameSaver.currData.scene).spawnpoint.GetValue().y - 10f;
            bound -= ink.GetComponent<MeshRenderer>().bounds.extents.y;
        }

        ink.transform.position = new Vector3(inkObj.transform.position.x, Mathf.Min(bound, height), inkObj.transform.position.z);
        ink.flooding = flooding;
        ink.curDest = destination;
        ink.couldSoftlock = couldSoftlock;
    }
}