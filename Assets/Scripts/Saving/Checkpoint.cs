using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Sprite flagpole;
    public Sprite with_flag; // Temp - will rewrite to animate raising/lowering later
    private bool has_triggered = false;
    public SoundPlayer soundPlayer;

    private void Start()
    {
        if (PlayerVars.instance.GetSpawnPos() == transform.position)
        {
            ActivateCheckpoint();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !has_triggered)
        {
            ActivateCheckpoint();
            soundPlayer.PlaySound("Level.Checkpoint");
        }
    }

    void ActivateCheckpoint()
    {
        GetComponent<SpriteRenderer>().sprite = with_flag;
        PlayerVars.instance.ReplenishTools();
        foreach (Tool t in PlayerVars.instance.inventory._toolUnlocks)
        {
            t._fuelEvent(t.GetFuelRemaining());
        }
        PlayerVars.instance.SetSpawnPos(transform.position);
        PlayerVars.instance.SaveInventory();
        GameSaver.SaveStickers();
        GameSaver.instance.SaveGame();
        has_triggered = true;
    }
}
