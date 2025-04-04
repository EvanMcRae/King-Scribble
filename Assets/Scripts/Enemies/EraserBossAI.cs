using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
using UnityEngine;

public class EraserBossAI : MonoBehaviour
{
    private IEnumerator chargingCooldown;
    [SerializeField] private GameObject PencilLinesFolder;
    
    private enum State {
        Searching, // search for a position
        Moving,
        ChargePrep,
        Charging, // for lines
        Dizzied,
        Damaged,
        SlamPrep, // hovering above KS before slam
        Slamming, // for KS on ground
        Nothing // testing
    }
    [SerializeField] public float speed = 3f; // Base speed
    [SerializeField] public float eraserRadius = 1f;
    private Vector2 currentPosition;
    public GameObject KingScribble;
    private GameObject target; // current target: either KS or a LineRenderer
    private Vector3 destination;
    private State state;
    private SpriteRenderer spriteRenderer;
    private float timer = 0.0f;
    private float searchTime = 3.0f;
    private float slamCooldownTime = 2.0f;
    private float slamPrepTime = 2.0f;

    void Start() {
        KingScribble = PlayerVars.instance.gameObject;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        timer += Time.deltaTime;

        switch (state) {
            default:
            case State.Searching:
                SearchForPosition();
                break;

            case State.Moving:
                Hover(target.transform.position);
                state = State.Nothing;
                break;

            case State.ChargePrep:
                destination = target.transform.position;
                break;
            
            case State.Charging:
                // get the line renderer position CLOSEST to boss.transform.position
                // MoveTo the first point of the LineRenderer and loop thru all the remaining positions to delete
                break;

            case State.SlamPrep:
                destination = target.transform.position;
                Hover(destination + new Vector3(0.0f, 10.0f)); // hover above KS
                if(timer >= slamPrepTime) {
                    timer = 0;
                    spriteRenderer.color = Color.red;
                    state = State.Slamming;
                }
                break;

            case State.Slamming:
                Slam(destination);
                EraserFunctions.Erase(transform.position, eraserRadius, false, PencilLinesFolder);
                break;

            case State.Nothing: // used for slamCoolDown
                if(timer >= slamCooldownTime) {
                    Color color;
                    if( ColorUtility.TryParseHtmlString("#FFB8EF", out color))
                    {
                        spriteRenderer.color = color;
                    }
                    timer = 0;
                    state = State.Searching;
                }
                break;
        }
    }

     void SearchForPosition() {
        // If line renderer present, goes for the biggest one
        target = KingScribble;
        if(timer >= searchTime){
            timer = 0;
            spriteRenderer.color = Color.yellow;
            state = State.SlamPrep;    
        }
    }

    void Hover(Vector2 destination) {
        float step = speed * Time.deltaTime; // Calculate the maxDistanceDelta based on the distance
        transform.position =  Vector2.MoveTowards(transform.position, destination, step);
    }

    void Slam(Vector2 destination) {
        float step = speed * Time.deltaTime; // Calculate the maxDistanceDelta based on the distance

        transform.position =  Vector2.MoveTowards(transform.position, destination, step);

        // If destination reached, start slam cool down
        if(Vector2.Distance(transform.position, destination) < .1f) {
            Debug.Log("Destination Reached");
            timer = 0;
            spriteRenderer.color = Color.yellow;
            state = State.Nothing;
        }
        // on collision with KS: deal damage to KS

        // on collsion with LineRenderer: erase

        // on collision with pen wall of enough mass: shatter wall and state = State.Dizzied
    }
}