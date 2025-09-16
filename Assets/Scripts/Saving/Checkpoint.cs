using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Sprite flagpole;
    public Sprite with_flag; // Temp - will rewrite to animate raising/lowering later
    private bool has_triggered = false;
    public SoundPlayer soundPlayer;
    public static Action ActivatedAction;

    private void Start()
    {
        if (Vector2.Distance(PlayerVars.instance.GetSpawnPos(), GetCheckpointPosition()) < 0.01f)
        {
            SetCheckpointTriggered();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !has_triggered && !PlayerVars.instance.isResetting && !PlayerVars.instance.isDead)
        {
            ActivateCheckpoint();
            soundPlayer.PlaySound("Level.Checkpoint");
        }
    }

    void ActivateCheckpoint() // to also save the game
    {
        SetCheckpointTriggered();
        PlayerVars.instance.SetSpawnPos(GetCheckpointPosition());
        PlayerVars.instance.SaveInventory();
        GameSaver.SaveStickers();
        GameSaver.instance.SaveGame(true);
        ActivatedAction?.Invoke();
    }

    void SetCheckpointTriggered()
    {
        GetComponent<SpriteRenderer>().sprite = with_flag;
        PlayerVars.instance.ReplenishTools();
        foreach (ToolType t in PlayerVars.instance.inventory._toolTypes)
        {
            Tool tool = DrawManager.GetTool(t);
            tool._fuelEvent(tool.GetFuelRemaining());
        }
        has_triggered = true;
    }

    Vector3 GetCheckpointPosition()
    {
        return transform.position - Vector3.up * GetComponent<SpriteRenderer>().sprite.bounds.extents.y;
    }
}
