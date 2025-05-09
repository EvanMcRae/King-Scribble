using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using TMPro;
using UnityEngine.Splines;
using Unity.VisualScripting;

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
        EndScene,
        Nothing
    }
    // serialized vars
    [SerializeField] private bool disable = false;
    [SerializeField] private GameObject PencilLinesFolder; // Where pencil lines are stored in hierarchy
    [SerializeField] private GameObject PenLinesFolder; // Where pen objs are stored in hierarchy
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject platform;
    [SerializeField] private TextMeshProUGUI myText;
    [SerializeField] GameObject leftChainL;
    [SerializeField] GameObject leftChainR;
    [SerializeField] GameObject rightChainL;
    [SerializeField] GameObject rightChainR;
    [SerializeField] EraserBossEvent eraserBossEvent;
    // behavior vars:
    private float baseSpeed = 30f; // Movement speed
    private float chargeSpeed = 50f;
    private float cooldownSpeed = 20f; // slower speed for cooldown to mimic tiredness
    private float eraserRadius = 1f; // Space that will be erased
    private float slamForce = 5000f; // assits with the slam "tween"
    private float knockbackForce = 20f; // upon KS hitting EB
    private float minDamageMass = 5.0f; // 10 works good
    private float roarForce = 200f;
    private int hitpoints = 3;
    private float maxPenArea = 100;
    private float totalPenArea = 0;
    private int minimumLinePoints = 10; // minimum number of points in a line for EB to want to erase it
    // common objects
    private State state;
    private GameObject KingScribble;
    private LineRenderer targetLine; // a LineRenderer in PencilLinesFolder
    private Vector3 destination; // position for where to move
    private SpriteRenderer spriteRenderer; // for animation
    private GameObject bounds1; // for Erase circle cast, requires 2 circle colliders
    private GameObject bounds2;
    private GameObject bounds3;
    private GameObject shieldSprite;
    private Collider2D KSCollider;
    private Rigidbody2D KSrb; // King Scribble's Rigidbody
    private CapsuleCollider2D physicalCollider; // EB's collider with physics
    private Rigidbody2D EBrb; // EB's Rigidbody2D
    private LineRenderer closestLine = null; // For searching
    

    // timer vars:
    private float timer = 0.0f; // used for cooldowns
    private float searchTime = 2.0f; // variables ending in "Time" relate to the timer
    private float slamCooldownTime = 2.0f;
    private float chargeCooldownTime = 1.0f;
    private float chargePrepTime = .66f;
    private float slamPrepTime = 2.0f;
    private float dizzyTime = 4.0f;
    private float damageTime = 2.0f;
    private float KSHitCooldown = 2.0f; // cooldown for how long until KS can be hit again
    private float KSStunTime = 2.0f; // time KS is stunned
    private float rotateTweenTime = 0.65f;
    // booleans:
    private bool isErasingLine = false; // booleans for states that are not independent enough for the state machine
    private bool isSlamming = false; // whether EB is in a tweening state
    private bool isRotated = false; // assists with EB's animations
    private bool isInvulnerable = false; // whether EB is invulnerable (force field)
    private bool isKSHit = false; // true when KS has been hit in general
    private bool isSlamHit = false; // true when KS has been hit by a slam
    private bool isShielding = false; // assists with starting the ActivateShield and RemoveShield coroutines
    
    // idk vars:
    private Tween rotateTween;
    Coroutine eraseLineSequence;
    private int numPoints; // line data
    private Vector3 targetPosition; // line data
    private bool firstIsClosest; // whether the first or last point is closer
    Vector3 lineUp;
    Vector3[] tempArray;
    Vector3 prevPosition;


    void Start() {

        DrawManager.instance.updatePenAreaEvent += updatePenArea;

        KingScribble = PlayerVars.instance.gameObject; // Initialize KS, his RigidBody2D, and MainBody trigger collider
        KSrb = KingScribble.GetComponent<Rigidbody2D>();
        EBrb = GetComponent<Rigidbody2D>(); 
        KSCollider = KingScribble.transform.Find("MainBody").GetComponent<PolygonCollider2D>(); // very iffy code
        spriteRenderer = transform.Find("EB_Sprite").GetComponent<SpriteRenderer>();
        shieldSprite = transform.Find("EB_Sprite/EB_Shield").gameObject;
        prevPosition = transform.position;

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
        
        // temp fix for shielding state! look into the logic abt why the state changes to SlamPrep before RemoveShield() enumerator ends
        if(isShielding) {
            anim.Play("EB_Idle");
            return;
        }

        timer += Time.deltaTime;
        Erase(); // should move Erase() to the designated states eventually!

        Vector3 direction = transform.position - prevPosition;
        //Debug.Log("direction: " + direction);
        
        // flip sprite appropriately
        if(direction.x > 0.01) {
            spriteRenderer.flipX = false;
        }
        if(direction.x < -0.01) {
            spriteRenderer.flipX = true;
        }

        prevPosition = transform.position;

        switch (state) {
            default:
            case State.Searching:
                anim.Play("EB_Idle");
                SearchForPosition();
                Hover(transform.position, 1f);
                break;

            case State.Moving:
                Hover(destination, baseSpeed);
                break;

            case State.ChargePrep:
                anim.Play("EB_Idle");
                
                // when destination reached, start windup
                Hover(destination, baseSpeed); // hover in the average direction of the line
                //Debug.Log("DISTANCE TO END POINT IS: " + Vector3.Distance(transform.position, destination));
                // explain to me unity why the lowest distance you can get with your movement function is 2.0 like where is the ACCURACY??
                if(Vector3.Distance(transform.position, destination) < 2.5 || timer > 5) {
                    //Debug.Log("Starting Windup");
                    timer = 0;
                    state = State.WindUp;
                }
                break;

            case State.WindUp:
                anim.Play("EB_WindUp");
                Hover(transform.position, 1.0f);
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
                Hover(transform.position, 1.0f);
                if(timer >= chargeCooldownTime) {
                    if(spriteRenderer.flipX == true) {
                        spriteRenderer.flipX = false;
                    }
                    timer = 0;
                    state = State.Searching;
                }
                break;

            case State.Dizzied:
                Hover(transform.position, 1f);
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
                }
                break;

            case State.SlamPrep:
                //Debug.Log("State = SlamPrep");
                anim.Play("EB_SlamPrep");
                spriteRenderer.flipX = false;
                Vector3 KSpos = KingScribble.transform.position;
                if(!isRotated) {
                    rotateTween = transform.DORotate(new Vector3(0,0,-90), rotateTweenTime);
                    isRotated = true;
                }
                Hover(new Vector3(KSpos.x, KSpos.y + 22.0f), baseSpeed); // hover above KS
                if(timer >= slamPrepTime) {
                    timer = 0;
                    destination = new Vector3(KSpos.x, -20.0f, KSpos.z); // y value should be below minimum floor
                    state = State.Slamming;
                }
                break;

            case State.Slamming:
                //Debug.Log("State = SLAMMING");
                anim.Play("EB_Slamming");
                Slam();
                break;
            
            case State.SlamImpact:
                //Debug.Log("State = SlamImpact");
                anim.Play("EB_SlamImpact");
                if(timer >= 1.0f) {
                    timer = 0;
                    state = State.SlamCooldown;
                }
                break;

            case State.SlamCooldown:
                anim.Play("EB_Idle");
                Hover(new Vector3(transform.position.x,-7f,0f), cooldownSpeed); // -7f is above the ground

                //if(timer >= 1.0f) {
                    rotateTween = transform.DORotate(new Vector3(0,0,0), rotateTweenTime);
                    isRotated = false;
                //}
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
                if(state == State.Charging && !isInvulnerable) {
                    Debug.Log("DIZZIED");
                    timer = 0;
                    StopCoroutine(eraseLineSequence); // stop the erasing coroutine
                    state = State.Dizzied;
                    isErasingLine = false;
                    Destroy(other.gameObject); // destroy pen object
                    other.GetComponent<Line>().deleted = true; // ensures the Line is deleted
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
                    other.GetComponent<Line>().deleted = true;
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

        // Ink waterfalls
        if(other.gameObject.layer == LayerMask.NameToLayer("EB_Hurt")) {
            // check for left or right pipe
            if(other.gameObject.name == "Flowing Ink Left") {
                if(!isShielding) {
                    StartCoroutine(RemoveShield(false));
                }
            }
            if(other.gameObject.name == "Flowing Ink Right") {
                if(!isShielding) {
                    StartCoroutine(RemoveShield(true));
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
        else if (other.gameObject.layer == LayerMask.NameToLayer("Water") && state == State.Slamming) {
            Debug.Log("WATER DETECTED, pos is: " + transform.position);
            EBrb.AddForce(new Vector2(0f, 1f * (slamForce - 10)), ForceMode2D.Impulse);
            timer = 0;
            isSlamming = false;
            isSlamHit = true;
            state = State.SlamImpact;
        }
        

        if (other.gameObject.layer == LayerMask.NameToLayer("PenLines") && (state == State.Slamming || state == State.SlamImpact)) {
            //Debug.Log("GROUND DETECTED, pos is: " + transform.position);
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
        // if(closestLine != null) {
        //     closestDistance = Vector3.Distance(transform.position, closestLine.transform.position);
        // }

        foreach (Transform childTransform in PencilLinesFolder.transform) // for each pencil line
        {
            LineRenderer tempLine = childTransform.GetComponent<LineRenderer>();
            if(tempLine.positionCount > minimumLinePoints) {
                // for each first and last point in the pencil line find which is the closest to EB
                float pointDistanceFirst = Vector3.Distance(transform.position, tempLine.GetPosition(0) + tempLine.transform.position); // first point in line
                float pointDistanceLast = Vector3.Distance(transform.position, tempLine.GetPosition(tempLine.positionCount - 1) + tempLine.transform.position); // last point in line
                

                if(pointDistanceFirst < closestDistance) {
                    closestDistance = pointDistanceFirst;
                    firstIsClosest = true;
                    closestLine = tempLine;
                }
                if(pointDistanceLast < closestDistance) {
                    closestDistance = pointDistanceLast;
                    firstIsClosest = false;
                    closestLine = tempLine;
                }
            }
        }

        if(closestLine != null) { // target is the closest line
            targetLine = closestLine;
            setLineData();
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
            //Debug.Log("APPLYING SLAM FORCE");
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

    
    // used to avoid null references to a line that will be erased
    private void setLineData() {
        numPoints = targetLine.positionCount;
        targetPosition = targetLine.transform.position;
        tempArray = new Vector3[numPoints];
        targetLine.GetPositions(tempArray); // get the positions into the array
        if(firstIsClosest) {
            lineUp = (targetLine.GetPosition(0) - targetLine.GetPosition(1)).normalized;  // this line sometimes bugs
            destination = targetLine.GetPosition(0) + targetLine.transform.position + (lineUp * 4); // position 0 in line renderer
        }
        else {
            lineUp = (targetLine.GetPosition(numPoints - 1) - targetLine.GetPosition(numPoints - 2)).normalized;
            destination = targetLine.GetPosition(numPoints-1) + targetLine.transform.position + (lineUp * 4); // position 0 in line renderer
        }
    }


    private IEnumerator EraseLineSequence(float speed) {
        // if (targetLine.positionCount == 0) yield break;

        isErasingLine = true;
        float step; // calculate the maxDistanceDelta based on the distance
        int mult = 3; // multipler if points need to be iterated not one by one

        if(firstIsClosest) {
            for(int i = 0; i < numPoints;) { // for each point in the pencil line move
                Vector3 point = tempArray[i] + targetPosition; // the destination
                step = speed * Time.fixedDeltaTime;
                

                EBrb.MovePosition(Vector2.MoveTowards(transform.position, point, step));
                
                //END POINT IS: " + Vector3.Distance(transform.position, point));
                if (Vector3.Distance(transform.position, point) < 2.5f) {  // ws > 0.01f
                    //Debug.LogWarning("increment i = " + i);
                    i+= mult; 
                }
                yield return new WaitForSeconds(0.01f); // wait for a bit... i think
            }
        }
        else {
            for(int i = numPoints - 1; i > -1;) { // for each point in the pencil line move
                Vector3 point = tempArray[i] + targetPosition; // the destination
                step = speed * Time.fixedDeltaTime;
                EBrb.MovePosition(Vector2.MoveTowards(transform.position, point, step));
                
                //Debug.Log("END POINT IS: " + Vector3.Distance(transform.position, point));
                if (Vector3.Distance(transform.position, point) < 2.5f) {  // ws > 0.01f
                    //Debug.LogWarning("increment i = " + i);
                    i-= mult; 
                }
                yield return new WaitForSeconds(0.01f); // wait for a bit... i think
            }
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
        // play animation
        yield return null;
    }

    // Despawns all pen objects in scene and knocks back KS off platform
    private IEnumerator ActivateShield() {
        Debug.Log("ACTIVATING SHIELD");
        isShielding = true;
        yield return new WaitForSeconds(1.0f);

        // Roar angry here! >:((
        isInvulnerable = true;
        shieldSprite.SetActive(true);
        yield return new WaitForSeconds(3.0f);
        state = State.Searching;
        isShielding = false;

        Debug.Log("ACTIVATING BUTTON");
        EraserBossEvent.ActivateButton();

        // break chain!

        DespawnAllPenObj();
        
    }

    private void updatePenArea(float area) {
        totalPenArea += area;
        Debug.Log("area: " + area);
        if(area > maxPenArea) {
            DespawnAllPenObj();
        }
    }

    private void DespawnAllPenObj() {
        //Debug.Log("DESPAWNING PEN OBJS");
        foreach (Transform childTransform in PenLinesFolder.transform) {
            StartCoroutine(DespawnPenObj(childTransform));
        }
    }
    
    private IEnumerator DespawnPenObj(Transform pen) {
        // play pen obj despawn warning animation
        // this code kinda sucks :(
        GameObject penObject = pen.gameObject;
        LineRenderer tempLine = pen.GetComponent<LineRenderer>();
        Color original = tempLine.startColor;
        Color opacity = new Color(tempLine.startColor.r, tempLine.startColor.g, tempLine.startColor.b, 0.5f);

        // Create 2 material blocks, one being the original and the other being transparent
        MeshRenderer polyRend = penObject.GetComponentInChildren<MeshRenderer>();
        MaterialPropertyBlock matBlockOG = new MaterialPropertyBlock();
        MaterialPropertyBlock matBlockOpacity = new MaterialPropertyBlock();
        polyRend.GetPropertyBlock(matBlockOpacity);
        polyRend.GetPropertyBlock(matBlockOG);
        matBlockOpacity.SetColor("_Color", opacity);

        // toggle opacities
        polyRend.SetPropertyBlock(matBlockOpacity);
        tempLine.startColor = opacity;
        tempLine.endColor = opacity;
        yield return new WaitForSeconds(.5f);
        polyRend.SetPropertyBlock(matBlockOG);
        tempLine.startColor = original;
        tempLine.endColor = original;
        yield return new WaitForSeconds(.5f);
        polyRend.SetPropertyBlock(matBlockOpacity);
        tempLine.startColor = opacity;
        tempLine.endColor = opacity;
        yield return new WaitForSeconds(.5f);
        polyRend.SetPropertyBlock(matBlockOG);
        tempLine.startColor = original;
        tempLine.endColor = original;
        yield return new WaitForSeconds(.5f);
        Destroy(pen.gameObject);
    }

    private void DespawnAllPencilObj() {
        //Debug.Log("DESPAWNING PENCIL OBJS");
        foreach (Transform childTransform in PencilLinesFolder.transform) {
            StartCoroutine(DespawnPencilObj(childTransform));
        }
    }
    

    private IEnumerator DespawnPencilObj(Transform pencil) {
        // play pen obj despawn warning animation
        // this code kinda sucks :(
        LineRenderer tempLine = pencil.GetComponent<LineRenderer>();
        Color original = tempLine.startColor;
        Color opacity = new Color(tempLine.startColor.r, tempLine.startColor.g, tempLine.startColor.b, 0.5f);

        // toggle opacities
        // ADD NULL CHECK cuz the line can be erased
        if(tempLine != null) {
            tempLine.startColor = opacity;
            tempLine.endColor = opacity;
            yield return new WaitForSeconds(.5f);
        }
        if(tempLine != null) {
            tempLine.startColor = original;
            tempLine.endColor = original;
            yield return new WaitForSeconds(.5f);
        }
        if(tempLine != null) {
            tempLine.startColor = opacity;
            tempLine.endColor = opacity;
            yield return new WaitForSeconds(.5f);
        }
        if(tempLine != null) {
            tempLine.startColor = original;
            tempLine.endColor = original;
            yield return new WaitForSeconds(.5f);
        }
        if(tempLine != null) {
            Destroy(pencil.gameObject);
        }
    }

    // Happens when the player pushes the button and EB gets hit with ink falling
    private IEnumerator RemoveShield(bool isRight) {
        state = State.Nothing;
        Debug.Log("DEACTIVATING SHIELD");
        isShielding = true;
        isInvulnerable = false;
        shieldSprite.SetActive(false);

        EBrb.AddForce(new Vector2(0f, 1f * slamForce), ForceMode2D.Impulse); // break pipe

        if(isRight) {
            eraserBossEvent.DeactivateRight(); // so ink cannot flow again from it
        }
        else {
            eraserBossEvent.DeactivateLeft();
        }


        //yield return new WaitForSeconds(2.0f); // how do we know when he is at the top...
        rotateTween = transform.DORotate(new Vector3(0,0,-90), rotateTweenTime);
        yield return new WaitForSeconds(1.0f);

        EBrb.AddForce(new Vector2(0f, -1f * slamForce), ForceMode2D.Impulse); // slamming
        Debug.Log("SHIELD SLAM FORCE ADDED");

        yield return new WaitForSeconds(.75f);
        // SLOW THE TIMEEEE
        Debug.Log("BREAKING CHAIN");
        if(isRight) { // break chain
            BreakRightChain();
        }
        else {
            BreakLeftChain();
        }
        
        
        DespawnAllPenObj();
        DespawnAllPencilObj();

        // play Roar animation
        yield return new WaitForSeconds(2.0f);
        rotateTween = transform.DORotate(new Vector3(0,0,0), rotateTweenTime);

        // Knockback KS to a wall!
        Vector3 distance = transform.position - KingScribble.transform.position;
        if(distance.x < 0) { // launch right
            Knockback(new Vector2(1f, .1f), roarForce);
        }
        else { // launch left 
            Knockback(new Vector2(-1f, .1f), roarForce);
        }

        //yield return new WaitForSeconds(1.0f);

        // cutscene behavior here! 
        EraserBossEvent.DeactivateButton(); // needs to be after the button is clear! otherwise the ink will continue flowing
        Debug.Log("EXITING REMOVESHIELD");
        timer = 0;
        state = State.Searching;
        isShielding = false;
    }


    private void BreakLeftChain() {
        leftChainL.GetComponent<BreakableChainLink>().Break();
        leftChainR.GetComponent<BreakableChainLink>().Break();
    }

    private void BreakRightChain() {
        rightChainL.GetComponent<BreakableChainLink>().Break();
        rightChainR.GetComponent<BreakableChainLink>().Break();
    }

    // increase the speeds and initiates shield:
    private void Difficulty2() {
        Debug.Log("INCREASING DIFFICULTY");
        baseSpeed += 10;
        chargeSpeed += 10;
    }

    // unbinds delegate upon destroying the eraser boss -- this is good practice!! - evan
    private void OnDestroy()
    {
        DrawManager.instance.updatePenAreaEvent -= updatePenArea;
    }
}