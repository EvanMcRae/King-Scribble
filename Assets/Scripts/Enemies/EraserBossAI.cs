using System.Collections;
using DG.Tweening;
using UnityEngine;
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
        SlamImpact, // hitting the ground (animation)
        SlamCooldown, // used for Slam Cooldown and Charge Cooldown
        EndScene
    }
    [SerializeField] private bool disable = false;
    [SerializeField] private GameObject PencilLinesFolder; // Where pencil lines are stored in hierarchy
    [SerializeField] private Animator anim;
    public float baseSpeed = 30f; // Movement speed
    private float cooldownSpeed = 10f; // slower speed for cooldown to mimic tiredness
    public float eraserRadius = 1f; // Space that will be erased
    public float knockbackForce = 10f; // upon KS hitting EB
    private float tweenTime = .33f; // the amount of time for the slam and charge tween to complete
    private float rotateTweenTime = 0.5f;
    public TextMeshProUGUI myText;
    private State state;
    public GameObject KingScribble;
    private LineRenderer targetLine; // a LineRenderer in PencilLinesFolder
    private Vector3 destination; // position for where to move
    private SpriteRenderer spriteRenderer; // for animation
    private GameObject bounds1; // for Erase circle cast, requires 2 circle colliders
    private GameObject bounds2;
    private Collider2D KSCollider;
    private float timer = 0.0f; // used for cooldowns
    private float searchTime = 3.0f; // variables ending in "Time" relate to the timer
    private float slamCooldownTime = 2.0f;
    private float chargeCooldownTime = 2.0f;
    private float slamPrepTime = 2.0f;
    private float dizzyTime = 3.0f;
    private float damageTime = 2.0f;
    private float KSHitCooldown = 2.0f; // cooldown for how long until KS can be hit again
    private float KSStunTime = 2.0f;
    private float minDamageMass = 5.0f; // 10 works good
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
    private int hitpoints = 3;

    Coroutine eraseLineSequence;

    void Start() {
        KingScribble = PlayerVars.instance.gameObject; // Initialize KS, his RigidBody2D, and MainBody trigger collider
        KSrb = KingScribble.GetComponent<Rigidbody2D>();
        KSCollider = KingScribble.transform.Find("MainBody").GetComponent<PolygonCollider2D>(); // very iffy code
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        bounds1 = transform.Find("Bounds1").gameObject; // initalize erasing colliders bounds
        bounds2 = transform.Find("Bounds2").gameObject;
        disable = false;
        

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
                anim.Play("EB_Idle");
                SearchForPosition();
                break;

            case State.Moving:
                break;

            case State.ChargePrep:
                anim.Play("EB_WindUp");
                
                destination = targetLine.GetPosition(0) + targetLine.transform.position; // position 0 in line renderer
                Hover(destination, baseSpeed); // hover in the average direction of the line
                if(timer >= slamPrepTime) {
                    timer = 0;
                    state = State.Charging;
                }
                break;
            
            case State.Charging:
                // use the closeset line and...
                // determine whether the first or last point of the line is closer and align himself with MoveTo
                anim.Play("EB_Dash");
                if(!isErasingLine) {
                    eraseLineSequence = StartCoroutine(EraseLineSequence(baseSpeed)); // start coroutine
                }
                break;

            case State.ChargeCooldown:
                anim.Play("EB_Idle");
                if(timer >= chargeCooldownTime) {
                    timer = 0;
                    state = State.Searching;
                }
                break;

            case State.Dizzied:
                anim.Play("EB_Stun");
                if(timer >= dizzyTime) {
                    state = State.Searching;
                }
                break;

            case State.Damaged:
                if(timer >= damageTime) {
                    state = State.Searching;
                }
                break;

            case State.SlamPrep:
                anim.Play("EB_SlamPrep");
                Vector3 KSpos = KingScribble.transform.position;
                if(!isRotated) {
                    slamTween = transform.DORotate(new Vector3(0,0,-90), rotateTweenTime);
                    isRotated = true;
                }
                Hover(new Vector3(KSpos.x, 15.0f), baseSpeed); // hover above KS
                if(timer >= slamPrepTime) {
                    timer = 0;
                    destination = new Vector3(KSpos.x, 1.0f, KSpos.z); // floor MUST be at 0!
                    state = State.Slamming;
                }
                break;

            case State.Slamming:
                anim.Play("EB_Slamming");
                Slam(destination, tweenTime);
                EraserFunctions.Erase(bounds1.transform.position, eraserRadius, false, PencilLinesFolder);
                EraserFunctions.Erase(bounds2.transform.position, eraserRadius, false, PencilLinesFolder);
                break;
            
            case State.SlamImpact:
                anim.Play("EB_SlamImpact");
                if(timer >= 1.0f) {
                    timer = 0;
                    state = State.SlamCooldown;
                }
                break;

            case State.SlamCooldown:
                anim.Play("EB_Idle");
                if(timer >= 1.0f) {
                    Hover(transform.position + new Vector3(0f,1f,0f), cooldownSpeed);
                    slamTween = transform.DORotate(new Vector3(0,0,0), rotateTweenTime);
                    isRotated = false;
                }
                if(timer >= slamCooldownTime) {
                    timer = 0;
                    state = State.Searching;
                }
                break;
            
            case State.EndScene:
                Hover(transform.position + new Vector3(1f,0f,0f), baseSpeed);
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {   
        //Debug.Log(other);
        if(other.CompareTag("Pen")) { // Get the RigidBody2D and compare its mass 
            GameObject penObj = other.gameObject;
            Rigidbody2D penRB = penObj.GetComponent<Rigidbody2D>();
            if(penRB.mass >= minDamageMass) { // Pen obj is big enough
                if(state == State.Charging) {
                    state = State.Dizzied;
                    StopCoroutine(eraseLineSequence); // stop the erasing coroutine
                    isErasingLine = false;
                    timer = 0;
                    // play dizzy animation
                    Destroy(other.gameObject); // destroy pen object
                }
                else if(state == State.Dizzied) {
                    // play damaged animation
                    hitpoints--;
                    Debug.Log("HP at " + hitpoints);
                    Destroy(other.gameObject); // destroy pen object (should I?)
                    timer = 0;
                    if(hitpoints == 0) {
                        state = State.EndScene;
                    }
                    else {
                        state = State.Damaged;
                    }
                }
            }
            else { // Pen obj is too small
                if(state == State.Charging) {
                    Destroy(other.gameObject); // destroy pen object
                }
            }
        }
        
        if(!isKSHit) {
            if (other == KSCollider) { // Deplete health from KS
                // Debug.Log("KS DETECTED");
                physicalCollider.enabled = false;
                PlayerVars.instance.SpendDoodleFuel(50);
                Vector3 distance = transform.position - KingScribble.transform.position;
                // Debug.Log("distance: " + distance);
                if(distance.x < 0) { // launch right
                    Knockback(new Vector2(1f, 1f));
                }
                else { // launch left 
                    Knockback(new Vector2(-1f, 1f));
                }
            }
        }

        // Stop at the ground when slamming, not at pencil lines
        // NOT IN USE -> SlamTweenComplete()
        // if (other.CompareTag("Ground") && other.gameObject.layer != LayerMask.NameToLayer("Lines") && state == State.Slamming) {
        //     Debug.Log("GROUND DETECTED");
        //     slamTween.Kill();
        //     timer = 0;
        //     isTweening = false;
        //     isSlamHit = true;
        //     state = State.SlamImpact;
        // }
    }

    void SearchForPosition() {
        // If line renderer present, goes for the biggest one OR closest one?
        float closestDistance = 100f;
        if(closestLineFound) {closestDistance = Vector3.Distance(this.transform.position, closestLine.transform.position);}

        foreach (Transform childTransform in PencilLinesFolder.transform) // for each pencil line
        {
            LineRenderer tempLine = childTransform.GetComponent<LineRenderer>();
            if(tempLine.positionCount > 1) {
                // for each point in the pencil line find which is the closest
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
            targetLine = closestLine;
        }

        if(timer >= searchTime){
            timer = 0;
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
            slamTween = transform.DOMove(destination, speed).SetEase(Ease.InCubic).OnComplete(()=>{slamTweenComplete();});
            slamTween.Play();
            isTweening = true;
        }
    }

    IEnumerator EraseLineSequence(float speed) {
        isErasingLine = true;
        float step; // Calculate the maxDistanceDelta based on the distance
        int numPoints = targetLine.positionCount;
        Vector3 targetPosition = targetLine.transform.position;

        if(targetLine.positionCount != 0) {
            Vector3[] tempArray = new Vector3[numPoints];
            Vector3 point;
            targetLine.GetPositions(tempArray); // Get the positions into the array

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
        state = State.ChargeCooldown;
        
    }

    private void slamTweenComplete() {
        Debug.Log("Slam Ended");
        timer = 0;
        isTweening = false;
        isSlamHit = true;
        state = State.SlamImpact;
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

    private IEnumerator StunPlayer(float duration) // Stun player movement upon slam
    {
        GameManager.canMove = false;
        yield return new WaitForSeconds(duration);
        GameManager.canMove = true;
        Debug.Log("UNFREEZE");
        yield return new WaitForSeconds(duration);
        Debug.Log("KNOCKBACK ENABLED");
        isKSHit = false;
    }

    private IEnumerator HitCooldown(float duration) { // Hit Cooldown when KS gets hit
        yield return new WaitForSeconds(duration);
        isKSHit = false;
        isSlamHit = false;
    }

    private IEnumerator Pause(float duration) {
        yield return new WaitForSeconds(duration);
    }

}