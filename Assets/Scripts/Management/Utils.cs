using System;
using UnityEngine;

public static class Utils
{
    // Functions in this class are accessible by all scripts in the current folder
    public static RaycastHit2D[] RaycastAll(Camera maincamera, Vector2 screenPosition, int layermask)
    {
        Ray ray = maincamera.ScreenPointToRay(screenPosition);
        RaycastHit2D[] hit2D = Physics2D.CircleCastAll(screenPosition, 0.5f, Vector2.zero, Mathf.Infinity, layermask); // returns all colliders in the circle
        Debug.DrawLine(screenPosition, screenPosition + new Vector2(0, 0.5f), Color.red);
        Debug.DrawLine(screenPosition, screenPosition + new Vector2(0, -0.5f), Color.red);
        Debug.DrawLine(screenPosition, screenPosition + new Vector2(0.5f, 0f), Color.red);
        Debug.DrawLine(screenPosition, screenPosition + new Vector2(-0.5f, 0f), Color.red);
        return hit2D;
    }

    public static CircleCollider2D Raycast(Camera maincamera, Vector2 screenPosition, int layermask)
    {
        Ray ray = maincamera.ScreenPointToRay(screenPosition);
        RaycastHit2D hit2D = Physics2D.CircleCast(screenPosition, 0.1f, Vector2.zero, Mathf.Infinity, layermask); // returns all colliders in the circle
        if (hit2D.collider != null) return (CircleCollider2D)hit2D.collider;

        return null;
    }

    public static void SetExclusiveAction(ref Action source, Action target)
    {
        if (source != null)
        {
            foreach (Action action in source.GetInvocationList())
            {
                source -= action;
            }
        }
        source += target;
    }
}
