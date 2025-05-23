using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterTriggerHandler : MonoBehaviour
{
    [SerializeField] private LayerMask _waterMask;

    private EdgeCollider2D _edgeColl;
    private InteractableWater _water;

    private void Awake()
    {
        _edgeColl = GetComponent<EdgeCollider2D>();
        _water = GetComponent<InteractableWater>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If collision gameObject is within the waterMask layerMask
        if ((_waterMask.value & (1 << collision.gameObject.layer)) > 0)
        {
            Rigidbody2D rb = collision.transform.root.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Clamp splash point to a MAX velocity
                int multiplier;
                if (rb.velocity.y < 0) { multiplier = -1; }
                else { multiplier = 1; }

                float vel = rb.velocity.y * _water.ForceMultiplier;
                vel = Mathf.Clamp(Mathf.Abs(vel), 0f, _water.MaxForce);
                vel *= multiplier;

                _water.Splash(collision, vel);
            }
        }
    }
}
