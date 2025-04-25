using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableChainLink : Breakable
{
    [SerializeField] private GameObject chain_base;

    public override void Break()
    {
        chain_base.GetComponent<DistanceJoint2D>().enabled = false;
        Destroy(gameObject);
    }

    /*
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 8)
        {
            chain_base.GetComponent<DistanceJoint2D>().enabled = false;
            Destroy(this.gameObject);
        }
    }
    */
}
