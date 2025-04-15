using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorScript : ChangeScene // Inherit from ChangeScene as a more specific scene-changing object
{
    const int NUM_SPRITES = 4; // There may be up to three locks - sprites exist for 3, 2, 1, and 0
    [SerializeField] private Sprite[] sprites = new Sprite[NUM_SPRITES]; // To easily change sprites when a lock is removed
    private Sprite sprite; // The current sprite for the door
    [SerializeField] private int numLocks; // The number of locks (if any) that must be removed to use the door 
    public bool keepTools; // If true, player will keep their tools when transitioning to the next stage through the door
    void Awake()
    {
        if (numLocks > 3) numLocks = 3; // A door may have no more than three locks
        sprite = sprites[numLocks];
        GetComponent<SpriteRenderer>().sprite = sprite;
    }
    public void removeLock() // Remove a lock from the door, called by each collected key that connects to the door
    {
        numLocks --;
        if (numLocks < 0) numLocks = 0;
        sprite = sprites[numLocks];
        GetComponent<SpriteRenderer>().sprite = sprite;
    }
    public override void OnTriggerEnter2D(Collider2D other) // Only change scene on interaction if no locks are remaining
    {
        if ((other.tag == "Player") && (numLocks == 0) && (!changingScene)) {
            GameSaver.currData.unlockedScenes.Add(SceneManager.GetActiveScene().name);
            if (keepTools) PlayerVars.instance.lastSavedInventory.copy(PlayerVars.instance.inventory);
            StartCoroutine(LoadNextScene());
        }
    }
}
