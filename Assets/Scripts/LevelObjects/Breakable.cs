using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour
{
    public virtual void Break()
    {
        Debug.Log("Generic Break");
    }
}
