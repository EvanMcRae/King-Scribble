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
    private Vector3 offset;
    void Start()
    {
        offset = new Vector3(0, -1f, 0);
        lastInstances = new GameObject[maxAmt];
        timeSinceLast = interval;
    }
    public void BeginDispensing()
    {
        dispensing = true;
    }
    public void StopDispensing()
    {
        dispensing = false;
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
                        timeSinceLast = 0f;
                        break;
                    }
                }
                /*
                if (lastInstance == null)
                {
                    lastInstance = Instantiate(item, transform.position + offset, Quaternion.identity);
                    timeSinceLast = 0f;
                }   
                */
            }
        }
    }
}
