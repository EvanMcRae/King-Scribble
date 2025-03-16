using UnityEngine;

public static class Utils
{
  // Functions in this class are accessible by all scripts in the current folder
  public static CircleCollider2D Raycast(Camera maincamera, Vector2 screenPosition, int layermask) {
    Ray ray = maincamera.ScreenPointToRay(screenPosition);
    RaycastHit2D hit2D = Physics2D.CircleCast(screenPosition, .01f, Vector2.zero, Mathf.Infinity, layermask); // returns the first collider along the ray
    if (hit2D.collider != null) return (CircleCollider2D) hit2D.collider;
    return null;
  }
  
}
