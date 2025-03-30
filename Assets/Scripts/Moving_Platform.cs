using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class Moving_Platform : MonoBehaviour
{
    public float moveSpeed = 1f;
    private float curSpeed;
    public float moveDist = 10f;
    public enum direction {Left, Up};
    public direction dir; // The direction to move
    public bool fastReturn; // True = platform will return twice as fast
    private Vector2 dest;
    public CinemachineVirtualCamera tempView; // The secondary virtual camera positioned to briefly show the entirety of the affected area (optional)
    public float viewTime = 2.5f; // The time the camera will linger on the secondary position before returning to the player (optional)
    // Start is called before the first frame update
    void Start()
    {
        dest = transform.position;
        curSpeed = moveSpeed;
    }

    public void MoveToDest()
    {
        curSpeed = moveSpeed;
        if (dir == direction.Left)
            dest.x += moveDist;
        else
            dest.y += moveDist;
    }
    public void ReturnToStart()
    {
        if (fastReturn) curSpeed = moveSpeed * 2;
        if (dir == direction.Left)
            dest.x -= moveDist;
        else
            dest.y -= moveDist;
        
    }
    public void PanCamera() // Optional - pans the camera temporarily to better show all platforms affected by the player's current action
    {
        GameManager.instance.SwitchCameras(PlayerController.instance.virtualCamera, tempView, viewTime);
    }
    // Update is called once per frame
    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, dest, curSpeed*Time.deltaTime);
    }
}
