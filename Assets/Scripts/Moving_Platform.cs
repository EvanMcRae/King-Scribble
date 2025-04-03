using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class Moving_Platform : MonoBehaviour
{
    public float moveSpeed = 1f;
    private float curSpeed;
    public float moveDist = 10f;
    public enum direction {Right, Up};
    public direction dir; // The direction to move
    public bool fastReturn; // True = platform will return twice as fast
    private Vector2 dest;
    private Vector2 start;
    private Vector2 col_dir;
    // private bool stopped = false;
    private bool move_stopped = false;
    private bool ret_stopped = false;
    private bool moving = false;
    private bool returning = false;
    public CinemachineVirtualCamera tempView; // The secondary virtual camera positioned to briefly show the entirety of the affected area (optional)
    public float viewTime = 2.5f; // The time the camera will linger on the secondary position before returning to the player (optional)
    // Start is called before the first frame update
    void Start()
    {
        start = transform.position;
        dest = transform.position;
        curSpeed = moveSpeed;
    }

    public void MoveToDest()
    {
        returning = false;
        curSpeed = moveSpeed;
        if (dir == direction.Right)
            dest.x += moveDist;
        else
            dest.y += moveDist;
        moving = true;
    }
    public void ReturnToStart()
    {
        moving = false;
        if (fastReturn) curSpeed = moveSpeed * 2;
        if (dir == direction.Right)
            dest.x -= moveDist;
        else
            dest.y -= moveDist;
        returning = true;   
    }
    public void PanCamera() // Optional - pans the camera temporarily to better show all platforms affected by the player's current action
    {
        GameManager.instance.SwitchCameras(PlayerController.instance.virtualCamera, tempView, viewTime);
    }
    public void OnCollisionStay2D(Collision2D other)
    {
        if (other.collider.gameObject.layer == 8 || other.collider.gameObject.layer == 3) // Ground = layer 3   Lines = layer 8
        {
            // X-value positive = collision from RIGHT      X-value negative = collision from LEFT
            // Y-value positive = collision from TOP        Y-value negative = collision from BOTTOM
            col_dir = (other.collider.gameObject.transform.position - transform.position).normalized;
            // dir = Right -> we only care about X-value
            if (dir == direction.Right)
            {
                // moveDist > 0 -> moving RIGHT, returning LEFT
                if (moveDist > 0f)
                {
                    if (col_dir.x > 0) move_stopped = true; // If collsion from RIGHT and moving RIGHT, prevent moving
                    else ret_stopped = true; // If collision from LEFT and moving RIGHT, prevent returning
                }
                // movedist < 0 -> moving LEFT, returning RIGHT
                else
                {
                    if (col_dir.x > 0) ret_stopped = true; // If collision from RIGHT and moving LEFT, prevent returning
                    else move_stopped = true; // If collision from LEFT and moving LEFT, prevent moving
                }
            }
            // dir = Up -> we only care about Y-value
            else if (dir == direction.Up)
            {
                // moveDist > 0 -> moving UP, returning DOWN
                if (moveDist > 0f)
                {
                    if (col_dir.y > 0) move_stopped = true; // If collision from UP and moving UP, prevent moving
                    else ret_stopped = true; // If collision from DOWN and moving UP, prevent returning
                }
                // moveDist < 0 -> moving DOWN, returning UP
                else
                {
                    if (col_dir.y > 0) ret_stopped = true; // If collision from UP and moving DOWN, prevent returning
                    else move_stopped = true; // If collision from DOWN and moving DOWN, prevent returning
                }
            }
        }
    }
    public void OnCollisionExit2D(Collision2D other)
    {
        if (other.collider.gameObject.layer == 8 || other.collider.gameObject.layer == 3) // Ground = layer 3   Lines = layer 8
        {
            // Since OnCollisionStay2D triggers every frame, any collision exiting can simply remove all move restrictions
            // As any applied by still-present collisions will be re-applied immediately
            move_stopped = false;
            ret_stopped = false;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if ((!(moving && move_stopped)) && (!(returning && ret_stopped)))
            transform.position = Vector2.MoveTowards(transform.position, dest, curSpeed*Time.deltaTime);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "PlayerMain") // Only one collider on the player prefab has this tag
        {
            if ((other.transform.parent != null) && (other.transform.parent.transform.parent != null))
                other.transform.parent.transform.parent.SetParent(transform);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "PlayerMain")
        {
            if ((other.transform.parent != null) && (other.transform.parent.transform.parent != null))
            {
                other.transform.parent.transform.parent.SetParent(null);
                DontDestroyOnLoad(other.transform.parent.transform.parent);
            }
        }
    }
}
