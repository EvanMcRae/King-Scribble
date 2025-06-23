using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;

public class WaterfallOpacity : MonoBehaviour
{
    [SerializeField] private Color initial;
    [SerializeField] private Color final;
    private Vector3 top;
    private Vector3 bot;
    void Start()
    {
        gameObject.GetComponent<SpriteShapeRenderer>().color = initial;
        top = gameObject.GetComponent<PolygonCollider2D>().bounds.max;
        bot = gameObject.GetComponent<PolygonCollider2D>().bounds.min;
    }

    // NOTE - maybe make this fade better (currently it fades too quickly while falling, should be more gradual, but it works for now)
    void Update()
    {
        Vector3 playerPos = PlayerController.instance.transform.position;
        Color color = gameObject.GetComponent<SpriteShapeRenderer>().color;
        if (top.y >= playerPos.y && playerPos.y >= bot.y) // Player within y-bounds of waterfall
        {
            Vector3 dir = bot - top;
            float alpha = initial.a * (1.25f - Vector3.Dot(playerPos - top, dir) / dir.sqrMagnitude * 0.5f);
            if (alpha > initial.a) alpha = initial.a;
            if (alpha < final.a) alpha = final.a;
            color.a = alpha;
            gameObject.GetComponent<SpriteShapeRenderer>().color = color;
        }
    }
}
