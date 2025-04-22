using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;

public class EraserBossAI : MonoBehaviour
{    
    private enum State {
        Searching, // search for a position
        Moving,
        ChargePrep,
        Charging, // for lines
        ChargeCooldown,
        Dizzied,
        Damaged,
        SlamPrep, // hovering above KS before slam
        Slamming, // for KS on ground
        SlamCooldown // used for Slam Cooldown and Charge Cooldown
    }
    [SerializeField] private bool disable = false;
    [SerializeField] private GameObject PencilLinesFolder; // Where pencil lines are stored in hierarchy
    public float baseSpeed = 30f; // Movement speed
    private float cooldownSpeed = 3f; // slower speed for cooldown to mimic tiredness
    public float eraserRadius = 1f; // Space that will be erased
    public float knockbackForce = 10f; // upon KS hitting EB
    private float tweenTime = .33f; // the amount of time for the slam and charge tween to complete
    public TextMeshProUGUI myText;
    private State state;
    public GameObject KingScribble;
    private Transform target; // current target's transform: this is either KS or a LineRenderer in PencilLinesFolder
    private Vector3 destination; // position for where to move
    private SpriteRenderer spriteRenderer; // for animation
    private GameObject bounds1; // for Erase circle cast, requires 2 circle colliders
    private GameObject bounds2;
    private Collider2D KSCollider;
    private float timer = 0.0f; // used for cooldowns
    private float searchTime = 3.0f; // variables ending in "Time" relate to the timer
    private float slamCooldownTime = 4.0f;
    private float chargeCooldownTime = 2.0f;
    private float slamPrepTime = 2.0f;
    private float KSHitCooldown = 2.0f; // cooldown for how long until KS can be hit again
    private float KSStunTime = 2.0f;
    private bool isErasingLine = false; // booleans for states that are not independent enough for the state machine
    private bool isTweening = false; // whether EB is in a tweening state
    private bool isRotated = false; // assists with EB's animations
    private bool isKSHit = false; // true when KS has been hit in general
    private bool isSlamHit = false; // true when KS has been hit by a slam

    private LineRenderer closestLine = null; // For searching
    private bool closestLineFound = false;

    private Tween slamTween;
    
    private Rigidbody2D KSrb; // King Scribble's Rigidbody
    private CapsuleCollider2D physicalCollider; // EB's collider with physics

    void Start() {
        KingScribble = PlayerVars.instance.gameObject;
        KSCollider = KingScribble.transform.Find("MainBody").GetComponent<PolygonCollider2D>(); // very iffy code
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        bounds1 = transform.Find("Bounds1").gameObject;
        bounds2 = transform.Find("Bounds2").gameObject;
        disable = false;
        KSrb = KingScribble.GetComponent<Rigidbody2D>();

        foreach (CapsuleCollider2D col in GetComponents<CapsuleCollider2D>()) // find EB's trigger collider (used for collision detection)
        {
            if (!col.isTrigger)
            {
                physicalCollider = col;
                break;
            }
        }
    }

    void FixedUpdate()
    {   
        myText.text = state.ToString();

        if(disable) return;
        timer += Time.deltaTime;
        
        switch (state) {
            default:
            case State.Searching:
                SearchForPosition();
                break;

            case State.Moving:
                Hover(target.position, baseSpeed);
                break;

            case State.ChargePrep:
                destination = target.position;
                Hover(destination, baseSpeed); // hover in the average direction of the line
                if(timer >= slamPrepTime) {
                    timer = 0;
                    spriteRenderer.color = Color.red;
                    state = State.Charging;
                }
                break;
            
            case State.Charging:
                // get the line renderer position CLOSEST to boss.transform.position
                // MoveTo the first point of the LineRenderer and loop thru all the remaining positions to delete
                if(!isErasingLine) {
                    StartCoroutine(EraseLineSequence(baseSpeed));
                }
                state = State.ChargeCooldown;
                break;

            case State.ChargeCooldown:
                if(timer >= chargeCooldownTime) {
                    Color color;
                    if(ColorUtility.TryParseHtmlString("#FFB8EF", out color))
                    {
                        spriteRenderer.color = color;
                    }
                    timer = 0;
                    state = State.Searching;
                }
                break;

            case State.SlamPrep:
                destination = target.position;
                if(!isRotated) {
                    transform.Rotate(0, 0, 90);
                    isRotated = true;
                }
                Hover(destination + new Vector3(0.0f, 10.0f), baseSpeed); // hover above KS
                if(timer >= slamPrepTime) {
                    timer = 0;
                    spriteRenderer.color = Color.red;
                    destination = new Vector3(destination.x, 0, destination.z); // floor MUST be at 0!
                    state = State.Slamming;
                }
                break;

            case State.Slamming:
                Slam(destination, tweenTime);
                EraserFunctions.Erase(bounds1.transform.position, eraserRadius, false, PencilLinesFolder);
                EraserFunctions.Erase(bounds2.transform.position, eraserRadius, false, PencilLinesFolder);
                break;

            case State.SlamCooldown:
                if(timer >= 1.0f) {
                    Hover(transform.position + new Vector3(0f,1f,0f), cooldownSpeed);
                }
                if(timer >= slamCooldownTime) {
                    Color color;
                    if(ColorUtility.TryParseHtmlString("#FFB8EF", out color))
                    {
                        spriteRenderer.color = color;
                    }
                    timer = 0;

                    if(isRotated) {
                        transform.Rotate(0, 0, -90);
                        isRotated = false;
                    }
                    state = State.Searching;
                }
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {   
        Debug.Log(other);
        
        if(state == State.Dizzied) {
            // is vulnerable

        }

        if(!isKSHit) {
            if (other == KSCollider) { // Deplete health from KS
                Debug.Log("KS DETECTED");
                physicalCollider.enabled = false;
                PlayerVars.instance.SpendDoodleFuel(50);
                Vector3 distance = transform.position - KingScribble.transform.position;
                Debug.Log("distance: " + distance);
                if(distance.x < 0) { // launch right
                    Knockback(new Vector2(1f, 1f));
                }
                else { // launch left 
                    Knockback(new Vector2(-1f, 1f));
                }
            }
        }

        // Stop at the ground when slamming, not at pencil lines ~ this can be refactored due to the floor logic y = 0
        if (other.CompareTag("Ground") && other.gameObject.layer != LayerMask.NameToLayer("Lines") && state == State.Slamming) {
            Debug.Log("GROUND DETECTED");
            slamTween.Kill();
            timer = 0;
            spriteRenderer.color = Color.yellow;
            isTweening = false;
            isSlamHit = true;
            state = State.SlamCooldown;
        }
        
        
        // Destroy pen obj is mass is big enough
        if (other.gameObject.layer == LayerMask.NameToLayer("Lines")) {
            // WIP
        }
    }

    void SearchForPosition() {
        // If line renderer present, goes for the biggest one OR closest one?
        float closestDistance = 100f;
        if(closestLineFound) {closestDistance = Vector3.Distance(this.transform.position, closestLine.transform.position);}

        foreach (Transform childTransform in PencilLinesFolder.transform) // for each pencil line
        {
            LineRenderer tempLine = childTransform.GetComponent<LineRenderer>();
            if(tempLine.positionCount > 1) {
                // for each point in the pencil line
                for(int i = 0; i < tempLine.positionCount; i++) {
                    float pointDistance = Vector3.Distance(this.transform.position, tempLine.GetPosition(i));
                    if(pointDistance < closestDistance) {
                        closestDistance = pointDistance;
                        closestLine = tempLine;
                        closestLineFound = true;
                    }
                }
            }
        }

        if(closestLineFound) { // target is the closest line
            target = closestLine.transform;
        }
        else {
            target = KingScribble.transform;
        }

        if(timer >= searchTime){
            timer = 0;
            spriteRenderer.color = Color.yellow;
            if(closestLineFound) {
                closestLineFound = false;
                state = State.ChargePrep;
            }
            else { state = State.SlamPrep; }    
        }
    }

    void Hover(Vector3 destination, float speed) {
        float step = speed * Time.deltaTime; // Calculate the maxDistanceDelta based on the distance
        transform.position =  Vector3.MoveTowards(transform.position, destination, step);
    }

    void Slam(Vector3 destination, float speed) {
        if (!isTweening) {    
            slamTween = transform.DOMove(destination, speed).SetEase(Ease.InCubic);
            slamTween.Play();
            isTweening = true;
        }
    }

    IEnumerator EraseLineSequence(float speed) {
        isErasingLine = true;
        float step; // Calculate the maxDistanceDelta based on the distance
        LineRenderer tempLine = target.GetComponent<LineRenderer>();
        int numPoints = tempLine.positionCount;
        Vector3 targetPosition = target.position;

        if(tempLine.positionCount != 0) {
            Vector3[] tempArray = new Vector3[numPoints];
            Vector3 point;
            tempLine.GetPositions(tempArray); // Get the positions into the array

            for(int i = 0; i < numPoints; i++) { // for each point in the pencil line move
                point = tempArray[i] + targetPosition;

                while (Vector3.Distance(transform.position, point) > 0.01f) {
                    step = speed * Time.deltaTime;
                    transform.position = Vector3.MoveTowards(transform.position, point, step);
                    yield return null; // wait for next frame
                }

                EraserFunctions.Erase(bounds1.transform.position, eraserRadius, false, PencilLinesFolder);
                EraserFunctions.Erase(bounds2.transform.position, eraserRadius, false, PencilLinesFolder);
            }
        }
        isErasingLine = false;
        timer = 0;
        spriteRenderer.color = Color.yellow;
        
    }

    private void Knockback(Vector2 knockbackDirection) {
        if (KSrb != null && !isKSHit) {
            Debug.Log("KNOCKING BACK");
            KSrb.AddForce(knockbackDirection * 20.0f, ForceMode2D.Impulse); // Use Impulse or VelocityChange
            isKSHit = true;
            print("state: " + state);
            if (isSlamHit) {
                StartCoroutine(StunPlayer(KSStunTime));
            }
            else {
                StartCoroutine(HitCooldown(KSHitCooldown));
            }
            
        }
        else { Debug.Log("RIGIDBODY NOT FOUND"); }
    }

    private IEnumerator StunPlayer(float duration)
    {
        GameManager.canMove = false;
        yield return new WaitForSeconds(duration);
        GameManager.canMove = true;
        Debug.Log("UNFREEZE");
        yield return new WaitForSeconds(duration);
        Debug.Log("KNOCKBACK ENABLED");
        isKSHit = false;
    }

    private IEnumerator HitCooldown(float duration) {
        yield return new WaitForSeconds(duration);
        isKSHit = false;
        isSlamHit = false;
    }

    private IEnumerator Pause(float duration) {
        yield return new WaitForSeconds(duration);
    }

}