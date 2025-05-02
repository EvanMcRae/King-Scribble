using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableChainLink : Breakable
{
    // Note: This is probably obvious, but please don't try to have the first or last link in the chain be breakable
    // I'm not sure why you would, but it won't work, and will probably do something really ugly.
    [SerializeField] private GameObject chain_base;
    [SerializeField] private GameObject upper_link; // The preceding link in the chain (from top to bottom)
    [SerializeField] private GameObject lower_link; // The next link in the chain (from top to bottom)
    [SerializeField] private GameObject ub_pref; // Prefab for the upper fragment of the broken chain
    [SerializeField] private GameObject lb_pref; // Prefab for the lower fragment of the broken chain
    public override void Break()
    {
        chain_base.GetComponent<DistanceJoint2D>().enabled = false;
        GameObject up = Instantiate(ub_pref, gameObject.transform.position, Quaternion.identity, gameObject.transform.parent);
        upper_link.GetComponent<HingeJoint2D>().connectedBody = up.GetComponent<Rigidbody2D>();
        upper_link.GetComponent<HingeJoint2D>().autoConfigureConnectedAnchor = false;
        upper_link.GetComponent<HingeJoint2D>().connectedAnchor = new(0, 0.2f);
        GameObject low = Instantiate(lb_pref, gameObject.transform.position, Quaternion.identity, gameObject.transform.parent);
        low.GetComponent<HingeJoint2D>().connectedBody = lower_link.GetComponent<Rigidbody2D>();
        low.GetComponent<HingeJoint2D>().connectedAnchor = new(0.01f, 0.03f); // No need to set autoConfigureConnectedAnchor - it is already set to false on the version we want to change
        Destroy(gameObject);
    }

}
