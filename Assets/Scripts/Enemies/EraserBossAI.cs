using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
using UnityEngine;

public class EraserBossAI : MonoBehaviour
{
    private enum State {
        Hovering,
        Moving,
        WindingUp,
        Charging,
        Dizzied,
        Damaged,
        Slamming,
        Nothing // testing
    }
    [SerializeField] public float speed = 3f; // Base speed
    [SerializeField] public float slowDownDistance = 5f; // Distance at which to start slowing down
    [SerializeField] public float maxSpeed = 5f; // Maximum speed
    private Vector2 currentPosition;
    public GameObject KingScribble;
    private GameObject target; // current target: either KS or a LineRenderer
    private Vector2 destination;
    private State state;
    private SpriteRenderer spriteRenderer;

    void Start() {
        KingScribble = GameObject.Find("Player(Clone)");
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        switch (state) {
            default:
            case State.Hovering:
                SearchForPosition();
                StartCoroutine(SearchTimer());
                break;

            case State.Moving:
                MoveTo(target.transform.position);
                state = State.Nothing;
                break;

            case State.WindingUp:
                destination = target.transform.position;
                spriteRenderer.color = Color.red;
                StartCoroutine(WindUpTimer());
                break;

            case State.Charging:
                MoveTo(destination);
                break;

            case State.Nothing:
                break;
        }
    }

    void MoveTo(Vector2 destination) {
        float distance = Vector2.Distance(transform.position, destination); // distance to target
        float maxDistanceDelta = speed * Time.deltaTime; // Calculate the maxDistanceDelta based on the distance

        if (distance <= slowDownDistance) { // Slow down as we get closer to the target
            maxDistanceDelta = Mathf.Lerp(0, maxSpeed, distance / slowDownDistance) * Time.deltaTime;
        }

        currentPosition = Vector2.MoveTowards(transform.position, destination, maxDistanceDelta); // move towards the target
        transform.position = currentPosition;

        // If destination reached, start charge cool down
        if(Vector2.Distance(transform.position, destination) < .1f) {
            StartCoroutine(ChargingCooldown());
        }

        // on collision with KS: deal damage to KS

        // on collsion with LineRenderer: erase

        // on collision with pen wall of enough mass: shatter wall and state = State.Dizzied
    }

    void SearchForPosition() {
        // If line renderer present, goes for the biggest one
        target = KingScribble;
    }

    // All coroutines below:

    private IEnumerator ChargingCooldown()
    {
        state = State.Nothing;
        Debug.Log("Charging Cooldown");
        spriteRenderer.color = Color.yellow;
        yield return new WaitForSeconds(3);
        // dumb code for hex values:
        Color color;
        if( ColorUtility.TryParseHtmlString("#FFB8EF", out color))
        {
            spriteRenderer.color = color;
        }
        state = State.Hovering;
    }

    private IEnumerator WindUpTimer()
    {
        state = State.Nothing;
        Debug.Log("WindUp timer");
        yield return new WaitForSeconds(2);
        state = State.Charging;
    }

    private IEnumerator SearchTimer()
    {
        state = State.Nothing;
        Debug.Log("Search timer");
        yield return new WaitForSeconds(3);
        state = State.WindingUp;
    }
}
