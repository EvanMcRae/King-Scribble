using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;
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
    public UnityEvent onFinishMove;
    public UnityEvent onFinishReturn;
    public GameObject gear_c;
    public GameObject gear_l;
    public GameObject gear_r;
    public float gearSpeed;
    public bool isWall = false;
    [SerializeField] private Rigidbody2D rigidBody;
    public Dictionary<Transform, Transform> passengerRoots = new();
    public Dictionary<Transform, PhysicsMaterial2D> physicsMats = new();

    void Start()
    {
        start = transform.position;
        dest = transform.position;
        curSpeed = moveSpeed;
        if (moveDist < 0) gearSpeed *= -1f; // If moving left, rotate the opposite direction on move/return
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

    void FixedUpdate()
    {
        if ((moving && !move_stopped) || (returning && !ret_stopped))
        {
            rigidBody.MovePosition(Vector2.MoveTowards(rigidBody.position, dest, curSpeed*Time.fixedDeltaTime));
        }
        if (moving && !move_stopped && !isWall && rigidBody.position != dest) 
        {
            gear_c.transform.rotation *= Quaternion.AngleAxis(-1f * gearSpeed * Time.deltaTime, Vector3.forward);
            gear_l.transform.rotation *= Quaternion.AngleAxis(-1f * gearSpeed * Time.deltaTime, Vector3.forward);
            gear_r.transform.rotation *= Quaternion.AngleAxis(-1f * gearSpeed * Time.deltaTime, Vector3.forward);
        }
        if (returning && !ret_stopped && !isWall && rigidBody.position != dest) 
        {
            gear_c.transform.rotation *= Quaternion.AngleAxis(gearSpeed * Time.deltaTime, Vector3.forward);
            gear_l.transform.rotation *= Quaternion.AngleAxis(gearSpeed * Time.deltaTime, Vector3.forward);
            gear_r.transform.rotation *= Quaternion.AngleAxis(gearSpeed * Time.deltaTime, Vector3.forward);
        }
        if (rigidBody.position == dest && moving)
        {
            onFinishMove.Invoke();
            moving = false;
        }
        else if (rigidBody.position == dest && returning)
        {
            onFinishReturn.Invoke();
            returning = false;
        }
    }
    
    void OnTriggerStay2D(Collider2D other)
    {
        if ((moving || returning) && (other.CompareTag("Player") || other.CompareTag("TempObj") || other.CompareTag("Pen")))
            Mount(other.gameObject);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("TempObj") || other.CompareTag("Pen"))
            Dismount(other.gameObject);
    }

    public void Mount(GameObject passenger)
    {
        if (passenger.transform.root != null && passenger.transform.root != transform.parent && !passenger.transform.root.CompareTag("MovPlat"))
        {
            passengerRoots.TryAdd(passenger.transform, passenger.transform.root);

            // Try to apply friction to mounted passengers (excluding players) so they stay on better
            if (passenger.transform.root.GetComponentInChildren<Rigidbody2D>() != null)
            {
                PhysicsMaterial2D mat = passenger.transform.root.GetComponentInChildren<Rigidbody2D>().sharedMaterial;
                physicsMats.TryAdd(passenger.transform, mat);

                if (!passenger.transform.CompareTag("Player"))
                {
                    passenger.transform.root.GetComponentInChildren<Rigidbody2D>().sharedMaterial = new PhysicsMaterial2D()
                    {
                        bounciness = mat != null ? mat.bounciness : 0f,
                        friction = 1f
                    };
                }
                else
                {
                    PlayerController.instance.SetFriction(true);
                }
            }
            
            passenger.transform.root.SetParent(transform.parent, true);
        }
    }

    public void Dismount(GameObject passenger)
    {
        if (gameObject.activeInHierarchy && passengerRoots.ContainsKey(passenger.transform))
        {
            passengerRoots[passenger.transform].GetComponentInChildren<Rigidbody2D>().sharedMaterial = physicsMats[passenger.transform];
            passengerRoots[passenger.transform].SetParent(null, true);
            if (passenger.CompareTag("Player") && passengerRoots[passenger.transform].gameObject.CompareTag("Player"))
            {
                DontDestroyOnLoad(passengerRoots[passenger.transform]);
                PlayerController.instance.SetFriction(false);
            }
            passengerRoots.Remove(passenger.transform);
            physicsMats.Remove(passenger.transform);
        }
    }
}
