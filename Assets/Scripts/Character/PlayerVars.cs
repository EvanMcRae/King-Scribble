using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVars : MonoBehaviour
{
    public Inventory inventory;
    void Start()
    {
        inventory = new Inventory();
    }
}
