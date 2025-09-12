using System;
using UnityEngine;

public static class Utils
{
    // TODO: Disable this for actual builds!!!
    public const bool CHEATS_ENABLED = true;

    public static Vector2[] ConvertArray(Vector3[] v3)
    {
        Vector2[] v2 = new Vector2[v3.Length];
        for (int i = 0; i < v3.Length; i++)
        {
            v2[i] = v3[i];
        }
        return v2;
    }

    // Functions in this class are accessible by all scripts in the current folder
    public static RaycastHit2D[] RaycastAll(Camera maincamera, Vector2 screenPosition, int layermask, float radius)
    {
        Ray ray = maincamera.ScreenPointToRay(screenPosition);
        RaycastHit2D[] hit2D = Physics2D.CircleCastAll(screenPosition, radius, Vector2.zero, Mathf.Infinity, layermask); // returns all colliders in the circle
        return hit2D;
    }

    public static CircleCollider2D Raycast(Camera maincamera, Vector2 screenPosition, int layermask, float radius)
    {
        Ray ray = maincamera.ScreenPointToRay(screenPosition);
        RaycastHit2D hit2D = Physics2D.CircleCast(screenPosition, radius, Vector2.zero, Mathf.Infinity, layermask); // returns all colliders in the circle
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
