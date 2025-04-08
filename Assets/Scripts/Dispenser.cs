using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dispenser : MonoBehaviour
{
    public GameObject item; // Item to dispense
    public float interval; // Interval at which to dispense item
    public int maxAmt; // Max amount of items allowed
    private float timeSinceLast;
    private bool dispensing = false;
    private GameObject[] lastInstances;
    public float m_offset;
    public float m_force;
    public enum Direction {Right, Up};
    public Direction m_direction;
    private Vector3 offset;
    private Vector2 force;
    const int ITEM_SPRITES = 8;
    public Sprite[] itemSprites = new Sprite[ITEM_SPRITES];
    public SoundPlayer soundPlayer;
    public SoundClip dispense;

    void Start()
    {

        lastInstances = new GameObject[maxAmt];
        timeSinceLast = interval;

        if (m_direction == Direction.Right)
        {
            offset = new Vector3(m_offset, 0f, 0f);
            force = new Vector2(m_force, 0f);
        }
            
        else if (m_direction == Direction.Up)
        {
            offset = new Vector3(0f, m_offset, 0f);
            force = new Vector2(0f, m_force);
        }
    }
    public void BeginDispensing()
    {
        dispensing = true;
    }
    public void StopDispensing()
    {
        dispensing = false;
    }
    public void DispenseOnce()
    {
        // Dispense once, but only if the current item limit has not been reached
        for (int i = 0; i < maxAmt; i++)
        {
            if (lastInstances[i] == null)
            {
                lastInstances[i] = Instantiate(item, transform.position + offset, Quaternion.identity);
                lastInstances[i].GetComponent<Rigidbody2D>().AddForce(force);
                if (soundPlayer != null && dispense != null)
                    soundPlayer.PlaySound(dispense);
                break;
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (dispensing) 
        {
            timeSinceLast += Time.deltaTime;
            if (timeSinceLast >= interval)
            {
                for (int i = 0; i < maxAmt; i++)
                {
                    if (lastInstances[i] == null)
                    {
                        lastInstances[i] = Instantiate(item, transform.position + offset, Quaternion.identity);
                        lastInstances[i].GetComponent<Rigidbody2D>().AddForce(force);
                        timeSinceLast = 0f;
                        if (soundPlayer != null && dispense != null)
                            soundPlayer.PlaySound(dispense);
                        break;
                    }
                }
            }
        }
    }
}
