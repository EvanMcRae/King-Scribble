//use this code to spawn in the prefab as a child to the object creating the effect
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockwaveSpawner : MonoBehaviour
{

    public GameObject shockPrefab;

    public void SpawnShockwave()
    {
        //spawns shockwave at parent location
        GameObject ShockwaveExist = Instantiate(shockPrefab, this.transform.position,
            this.transform.rotation, this.transform);

        if (ShockwaveExist != null)
        {
            //reference the shockwave shader script itself
            ShockwaveMan shockwaveManScript = ShockwaveExist.GetComponent<ShockwaveMan>();

            if (shockwaveManScript != null)
            {
                shockwaveManScript.CallShockwave();
            }
        }
    }
}
