using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Sprite flagpole;
    public Sprite with_flag; // Temp - will rewrite to animate raising/lowering later
    private bool has_triggered = false;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !has_triggered)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = with_flag;
            PlayerVars.instance.MaxDoodleFuel();
            PlayerVars.instance.SetSpawnPos(transform.position);
            has_triggered = true;
        }
    }
}
