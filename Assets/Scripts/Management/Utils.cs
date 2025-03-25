using UnityEngine;

public static class Utils
{
    // Functions in this class are accessible by all scripts in the current folder
    public static GameObject Raycast(Camera maincamera, Vector2 screenPosition, int layermask) 
    {
        Ray ray = maincamera.ScreenPointToRay(screenPosition);
        RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray, Mathf.Infinity, layermask); // returns the first collider along the ray
        if (hit2D.collider != null) return hit2D.collider.gameObject;
        return null;
    }
  
}
