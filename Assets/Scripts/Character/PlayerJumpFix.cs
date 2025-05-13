using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpFix : MonoBehaviour
{
    public void JustLanded()
    {
        PlayerController.instance.NotJumping();
    }
}
