using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(BuoyancyEffector2D))]
public class WaterBuoyancy : MonoBehaviour
{
    public float killThreshold = 0.5f;
    void OnTriggerStay2D(Collider2D collision)
    {
        if (!GameManager.resetting && collision.gameObject.CompareTag("Player") && collision.gameObject.name != "LandCheck")
        {
            PlayerVars.instance.GetComponent<Rigidbody2D>().mass = 10f;
            //Debug.Log(transform.position.y - collision.transform.position.y + GetComponent<BuoyancyEffector2D>().surfaceLevel);
            if (transform.position.y - collision.transform.position.y + GetComponent<BuoyancyEffector2D>().surfaceLevel > killThreshold && !PlayerVars.instance.cheatMode)
            {
                GetComponent<BuoyancyEffector2D>().density = 0.1f;
                GameManager.instance.Reset();
                PlayerVars.instance.GetComponent<Rigidbody2D>().mass = 1f;
            }
        }
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerVars.instance.GetComponent<Rigidbody2D>().mass = 1f;
        }
    }
}
