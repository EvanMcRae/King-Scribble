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

        //reference the shockwave shader script itself
        ShockwaveMan shockwaveManScript = ShockwaveExist.GetComponent<ShockwaveMan>();

        //Debug.Log("HEWWO UWU");
        //shockwaveManScript.Invoke(nameof(shockwaveManScript.CallShockwave), 10 / 12f);
        shockwaveManScript.CallShockwave();
        //Destroy(ShockwaveExist);
    }

   
}
