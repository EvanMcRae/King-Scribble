using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Pulley : MonoBehaviour
{
    [SerializeField] private GameObject p_left; // Left pulley's base
    [SerializeField] private GameObject p_right; // Right pulley's base
    
    void FixedUpdate()
    {
        p_left.GetComponent<Rigidbody2D>().AddForce(p_right.GetComponent<TargetJoint2D>().reactionForce);
        p_right.GetComponent<Rigidbody2D>().AddForce(p_left.GetComponent<TargetJoint2D>().reactionForce);
    }

}
