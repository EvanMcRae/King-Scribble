using System.Collections;
using DG.Tweening;
using UnityEngine;
using TMPro;
using UnityEngine.Splines;

/*
General notes:

EB has 2 capsule colliders, one isTrigger and the other is not:
    to disable Rigidbody2D collision with objects in the scene, go to the NOT isTrigger and exclude these layers
*/

public class EraserBossAI : MonoBehaviour
{    
    private enum State {
        Searching, // search for a position
        Moving,
        ChargePrep,
        WindUp, // for animation
        Charging, // for lines
        ChargeCooldown,
        Dizzied,
        Damaged,
        PenInk,
        Roar,
        SlamPrep, // hovering above KS before slam
        Slamming, // for KS on ground
        SlamImpact, // hitting the ground (animation)
        SlamCooldown, // used for Slam Cooldown and Charge Cooldown
        EndScene
    }
    [SerializeField] private bool disable = false;
    [SerializeField] private GameObject PencilLinesFolder; // Where pencil lines are stored in hierarchy
    [SerializeField] private GameObject PenLinesFolder; // Where pen objs are stored in hierarchy
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject platform;
    private float baseSpeed = 30f; // Movement speed
    private float chargeSpeed = 50f;
    private float cooldownSpeed = 10f; // slower speed for cooldown to mimic tiredness
    private float eraserRadius = 1f; // Space that will be erased
    private float slamForce = 5000f; // assits with the slam "tween"
    private float knockbackForce = 20f; // upon KS hitting EB
    private float roarForce = 100f;
    private float rotateTweenTime = 0.5f;
    [SerializeField] private TextMeshProUGUI myText;
    private State state;
    private GameObject KingScribble;
    private LineRenderer targetLine; // a LineRenderer in PencilLinesFolder
    private Vector3 destination; // position for where to move
    private SpriteRenderer spriteRenderer; // for animation
    private GameObject bounds1; // for Erase circle cast, requires 2 circle colliders
    private GameObject bounds2;
    private GameObject bounds3;
    private Collider2D KSCollider;
    private float timer = 0.0f; // used for cooldowns
    private float searchTime = 2.0f; // variables ending in "Time" relate to the timer
    private float slamCooldownTime = 1.0f;
    private float chargeCooldownTime = 1.0f;
    private float chargePrepTime = .66f;
    private float slamPrepTime = 2.0f;
    private float dizzyTime = 4.0f;
    private float damageTime = 2.0f;
    private float KSHitCooldown = 2.0f; // cooldown for how long until KS can be hit again
    private float KSStunTime = 2.0f; // time KS is stunned
    private float minDamageMass = 5.0f; // 10 works good
    private bool isErasingLine = false; // booleans for states that are not independent enough for the state machine
    private bool isSlamming = false; // whether EB is in a tweening state
    private bool isRotated = false; // assists with EB's animations
    private bool isInvulnerable = false; // whether EB is invulnerable (force field)
    private bool isKSHit = false; // true when KS has been hit in general
    private bool isSlamHit = false; // true when KS has been hit by a slam
    private bool isShielding = false;

    private LineRenderer closestLine = null; // For searching

    private Tween rotateTween;
    
    private Rigidbody2D KSrb; // King Scribble's Rigidbody
    private CapsuleCollider2D physicalCollider; // EB's collider with physics
    private Rigidbody2D EBrb; // EB's Rigidbody2D
    private int hitpoints = 3;
    private float totalPenMass = 0;

    Coroutine eraseLineSequence;

    void Start() {
        KingScribble = PlayerVars.instance.gameObject; // Initialize KS, his RigidBody2D, and MainBody trigger collider
        KSrb = KingScribble.GetComponent<Rigidbody2D>();
        EBrb = GetComponent<Rigidbody2D>(); 
        KSCollider = KingScribble.transform.Find("MainBody").GetComponent<PolygonCollider2D>(); // very iffy code
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        bounds1 = transform.Find("Bounds1").gameObject; // initalize erasing colliders bounds
        bounds2 = transform.Find("Bounds2").gameObject;
        bounds3 = transform.Find("Bounds3").gameObject;

        BoxCollider2D platformCol = platform.GetComponent<BoxCollider2D>();
        foreach (CapsuleCollider2D col in GetComponents<CapsuleCollider2D>()) // find EB's trigger collider (used for collision detection)
        {
            if (!col.isTrigger)
            {
                physicalCollider = col;
                Physics2D.IgnoreCollision(physicalCollider, platformCol); // ignore collision between EB and the center platform
                Debug.Log($"Ignoring collision between {col.name} and {platformCol.name}");
                Debug.Log("disabling physics");
            }
            else { // isTrigger collider
                Physics2D.IgnoreCollision(col, platformCol);
                Debug.Log($"Ignoring collision between {col.name} and {platformCol.name}");
                Debug.Log("disabling physics isTrigger");
            }
        }
    }

    void FixedUpdate()
    {   
        myText.text = state.ToString();

        if(disable) return;
        timer += Time.deltaTime;
        Erase(); // should move Erase() to the designated states eventually!
        
        switch (state) {
            default:
            case State.Searching:
                anim.Play("EB_Idle");
                SearchForPosition();
                break;

            case State.Moving:
                break;

            case State.ChargePrep:
                // Position self in line with the pos(0) and pos(1) if closer
                if(targetLine == null) {
                    state = State.Searching;
                }
                Vector3 lineUp = (targetLine.GetPosition(0) - targetLine.GetPosition(1)).normalized;

                anim.Play("EB_Idle");
                if(lineUp.x > 0) {
                    spriteRenderer.flipX = true;
                }
                else {
                    spriteRenderer.flipX = false;
                }  
                destination = targetLine.GetPosition(0) + targetLine.transform.position + (lineUp * 4); // position 0 in line renderer
                
                // when destination reached, start windup
                Hover(destination, baseSpeed); // hover in the average direction of the line
                Debug.Log("DISTANCE TO END POINT IS: " + Vector3.Distance(transform.position, destination));
                // explain to me unity why the lowest distance you can get with your movement function is 2.0 like where is the ACCURACY??
                if(Vector3.Distance(transform.position, destination) < 2.5 || timer > 5) {
                    Debug.Log("Starting Windup");
                    timer = 0;
                    state = State.WindUp;
                }
                break;

            case State.WindUp:
                anim.Play("EB_WindUp");
                if(timer >= chargePrepTime) {
                    timer = 0;
                    state = State.Charging;
                }
                break;

            
            case State.Charging:
                // use the closeset line: determine whether the first or last point of the line is closer and align himself with MoveTo
                anim.Play("EB_Dash");
                if(!isErasingLine) {
                    eraseLineSequence = StartCoroutine(EraseLineSequence(chargeSpeed)); // start coroutine
                }
                break;

            case State.ChargeCooldown:
                anim.Play("EB_Stop");
                if(timer >= chargeCooldownTime) {
                    if(spriteRenderer.flipX == true) {
                        spriteRenderer.flipX = false;
                    }
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
                    if(!isShielding) {
                        StartCoroutine(ActivateShield());
                    }
                   
                    // state = State.Searching;
                }
                break;

            case State.SlamPrep:
                anim.Play("EB_SlamPrep");
                Vector3 KSpos = KingScribble.transform.position;
                if(!isRotated) {
                    rotateTween = transform.DORotate(new Vector3(0,0,-90), rotateTweenTime);
                    isRotated = true;
                }
                Hover(new Vector3(KSpos.x, KSpos.y + 20.0f), baseSpeed); // hover above KS
                if(timer >= slamPrepTime) {
                    timer = 0;
                    destination = new Vector3(KSpos.x, -20.0f, KSpos.z); // y value should be below minimum floor
                    state = State.Slamming;
                }
                break;

            case State.Slamming:
                anim.Play("EB_Slamming");
                Slam();
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
                    rotateTween = transform.DORotate(new Vector3(0,0,0), rotateTweenTime);
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
        // Debug.Log(other);
        if(other.CompareTag("Pen") && !other.GetComponent<Line>().deleted) { // Get the RigidBody2D and compare its mass 
            GameObject penObj = other.gameObject;
            Rigidbody2D penRB = penObj.GetComponent<Rigidbody2D>();
            if(penRB.mass >= minDamageMass) { // Pen obj is big enough
                if(state == State.Charging) {
                    Debug.Log("DIZZIED");
                    timer = 0;
                    state = State.Dizzied;
                    StopCoroutine(eraseLineSequence); // stop the erasing coroutine
                    isErasingLine = false;
                    Destroy(other.gameObject); // destroy pen object
                    other.GetComponent<Line>().deleted = true;
                }
                else if(state == State.Dizzied) {
                    // play damaged animation
                    hitpoints--;
                    Debug.Log("HP at " + hitpoints);
                    Destroy(other.gameObject); // destroy pen object
                    other.GetComponent<Line>().deleted = true;
                    timer = 0;
                    if(hitpoints == 0) {
                        state = State.EndScene;
                    }
                    else {
                        state = State.Damaged;
                        Difficulty2();
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
                PlayerVars.instance.SpendDoodleFuel(50);
                Vector3 distance = transform.position - KingScribble.transform.position;
                if(distance.x < 0) { // launch right
                    Knockback(new Vector2(1f, 1f), knockbackForce);
                }
                else { // launch left 
                    Knockback(new Vector2(-1f, 1f), knockbackForce);
                }
            }
        }

        // Stop at the ground when slamming, not at pencil lines
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground") && state == State.Slamming) {
            Debug.Log("GROUND DETECTED, pos is: " + transform.position);
            timer = 0;
            isSlamming = false;
            isSlamHit = true;
            state = State.SlamImpact;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("PenLines") && state == State.Slamming) {
            Debug.Log("GROUND DETECTED, pos is: " + transform.position);
            Destroy(other.gameObject);
            timer = 0;
            isSlamming = false;
            isSlamHit = true;
            state = State.SlamImpact;
        }
    }

    void SearchForPosition() {
        // If line renderer present, goes for the biggest one OR closest one?
        float closestDistance = 100f;
        if(closestLine != null) {
            closestDistance = Vector3.Distance(transform.position, closestLine.transform.position);
        }

        foreach (Transform childTransform in PencilLinesFolder.transform) // for each pencil line
        {
            LineRenderer tempLine = childTransform.GetComponent<LineRenderer>();
            if(tempLine.positionCount > 1) {
                // for each first and last point in the pencil line find which is the closest to EB
                for(int i = 0; i < tempLine.positionCount; i++) {
                    float pointDistanceFirst = Vector3.Distance(transform.position, tempLine.GetPosition(i)); // first point in line
                    float pointDistanceLast = Vector3.Distance(transform.position, tempLine.GetPosition(tempLine.positionCount - 1)); // last point in line
                    if(pointDistanceFirst < closestDistance) {
                        closestDistance = pointDistanceFirst;
                        closestLine = tempLine;
                    }
                    if(pointDistanceLast < closestDistance) {
                        closestDistance = pointDistanceLast;
                        closestLine = tempLine;
                    }
                }
            }
        }

        if(closestLine != null) { // target is the closest line
            targetLine = closestLine;
        }

        if(timer >= searchTime){
            timer = 0;
            if(closestLine != null) {
                state = State.ChargePrep;
            }
            else { state = State.SlamPrep; }    
        }
    }

    void Hover(Vector3 destination, float speed) {
        float step = speed * Time.deltaTime; // Calculate the maxDistanceDelta based on the distance
        EBrb.MovePosition(Vector2.MoveTowards(transform.position, destination, step));
    }

    void Slam() {
        if (!isSlamming) {    
            Debug.Log("APPLYING SLAM FORCE");
            EBrb.AddForce(new Vector2(0f, -1f * slamForce), ForceMode2D.Impulse);
            isSlamming = true;
        }
    }

    // Takes into account EB's circlecast colliders
    void Erase() {
        EraserFunctions.Erase(bounds1.transform.position, eraserRadius, true, PencilLinesFolder);
        EraserFunctions.Erase(bounds2.transform.position, eraserRadius, true, PencilLinesFolder);
        EraserFunctions.Erase(bounds3.transform.position, eraserRadius, true, PencilLinesFolder);
    }

    private IEnumerator EraseLineSequence(float speed) {
        if (targetLine.positionCount == 0) yield break;

        isErasingLine = true;
        float step; // calculate the maxDistanceDelta based on the distance
        int numPoints = targetLine.positionCount;
        Vector3 targetPosition = targetLine.transform.position;

        Vector3[] tempArray = new Vector3[numPoints];
        targetLine.GetPositions(tempArray); // get the positions into the array
        int mult = 1; // multipler if points need to be iterated not one by one

        for(int i = 0; i < numPoints;) { // for each point in the pencil line move
            Vector3 point = tempArray[i] + targetPosition; // the destination
            step = speed * Time.fixedDeltaTime;
            EBrb.MovePosition(Vector2.MoveTowards(transform.position, point, step));
            
            //END POINT IS: " + Vector3.Distance(transform.position, point));
            if (Vector3.Distance(transform.position, point) < 2.5f) {  // ws > 0.01f
                //Debug.LogWarning("increment i = " + i);
                i+= mult; 
            }
            yield return null; // wait for a bit... i think
        }

        Debug.LogWarning("EXITED FOR LOOP " + Vector3.Distance(transform.position, tempArray[numPoints - 1] + targetPosition));
        //if(Vector3.Distance(transform.position, tempArray[numPoints - 1] + targetPosition) < 2 || timer > 5) {
        isErasingLine = false;
        timer = 0;
        state = State.ChargeCooldown; 
    }


    private void Knockback(Vector2 knockbackDirection, float force) {
        if (KSrb != null && !isKSHit) {
            Debug.Log("KNOCKING BACK");
            KSrb.AddForce(knockbackDirection * force, ForceMode2D.Impulse); // Use Impulse or VelocityChange
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

    private IEnumerator Pause(float duration) { // General pause function
        yield return new WaitForSeconds(duration);
    }


    private IEnumerator Roar() {
        yield return null;
    }

    // Despawns all pen objects in scene and knocks back KS off platform
    private IEnumerator ActivateShield() {
        isShielding = true;
        spriteRenderer.color = Color.red;
        state = State.Searching;
        isShielding = false;

        // toggle button activation here!
        // break chain!

        foreach (Transform childTransform in PenLinesFolder.transform) {
            // play pen obj despawn warning animation
            SpriteRenderer temp = childTransform.GetComponent<SpriteRenderer>();
            temp.color = Color.red;
            yield return new WaitForSeconds(3.0f);
            Destroy(childTransform.gameObject);
        }
    }
    
    // Happens when the player pushes the button and EB gets hit with ink falling
    private IEnumerator RemoveShield() {
        Debug.Log("DEACTIVATING SHIELD");
        Vector3 distance = transform.position - KingScribble.transform.position;
        if(distance.x < 0) { // launch right
            Knockback(new Vector2(1f, 1f), roarForce);
        }
        else { // launch left 
            Knockback(new Vector2(-1f, 1f), roarForce);
        }

        // cutscene behavior here!

        yield return new WaitForSeconds(3.0f);

        state = State.Searching;
    }

    // increase the speeds and initiates shield:
    private void Difficulty2() {
        isInvulnerable = true;
        baseSpeed += 10;
        chargeSpeed += 10;
    }
}