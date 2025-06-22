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
    private enum State
    {
        Start,
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
        ShieldActivate,
        ShieldRemove,
        SlamPrep, // hovering above KS before slam
        Slamming, // for KS on ground
        SlamImpact, // hitting the ground (animation)
        SlamCooldown, // used for Slam Cooldown and Charge Cooldown
        EndScene,
        Idle,
        Nothing
    }
    // serialized vars
    [SerializeField] private bool disable = false;
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject platform;
    [SerializeField] private TextMeshProUGUI myText;
    [SerializeField] GameObject leftChainL;
    [SerializeField] GameObject leftChainR;
    [SerializeField] GameObject rightChainL;
    [SerializeField] GameObject rightChainR;
    [SerializeField] EraserBossEvent eraserBossEvent;
    [SerializeField] private PhysicsMaterial2D slippery, friction;
    [SerializeField] private SpriteRenderer spriteRenderer; // for animation
    [SerializeField] private GameObject bounds1; // for Erase circle cast, requires 3 circle colliders
    [SerializeField] private GameObject bounds2;
    [SerializeField] private GameObject bounds3;
    [SerializeField] private GameObject shieldSprite;
    [SerializeField] private SoundPlayer soundPlayer;
    [SerializeField] private GameObject _leftInk;
    [SerializeField] private GameObject _rightInk;
    [SerializeField] private Moving_Platform movingWall;
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
    private float totalPenArea = 0; // all pen objects area added together
    private int minimumLinePoints = 10; // minimum number of points in a line for EB to want to erase it
    // common objects
    private State state;
    private GameObject KingScribble;
    private LineRenderer targetLine; // a LineRenderer in PencilLinesFolder
    private Vector3 destination; // position for where to move
    private Collider2D KSCollider;
    private Rigidbody2D KSrb; // King Scribble's Rigidbody
    private CapsuleCollider2D physicalCollider; // EB's collider with physics
    private Rigidbody2D EBrb; // EB's Rigidbody2D
    private LineRenderer closestLine = null; // For searching

    // timer vars:
    private float timer = 0.0f; // used for cooldowns
    private float oscillationTimer = 0.0f; // used for oscillation
    private float searchTime = 2.0f; // variables ending in "Time" relate to the timer
    private float slamCooldownTime = 1.0f;
    private float chargeCooldownTime = 1.0f;
    private float chargePrepTime = .66f;
    private float slamPrepTime = 2.5f;
    private float dizzyTime = 4.0f;
    private float damageTime = 2.0f;
    private float KSHitCooldown = 2.0f; // cooldown for how long until KS can be hit again
    private float KSStunTime = 2.0f; // time KS is stunned
    private float rotateTweenTime = 0.65f;
    // booleans:
    //      for states that are not independent enough for the state machine
    private bool isErasingLine = false; // assists with calling the coroutine
    private bool isSlamming = false; // assists with applying the slam force only once
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
    Vector3 lineUp; // position where EB will go before charging
    Vector3[] tempArray;
    Vector3 prevPosition;
    private bool faceRight; // the direction EB should face when performing the windup animation
    private float oscillation = 3000;
    private float swap = 1;
    private Vector3 KSpos;


    // ------------ SHOCKWAVE STUFF  ------------
    private ShockwaveSpawner spawnerScript;
    [SerializeField] private Camera _shockwaveCamera;
    [SerializeField] private Material _shockwaveMat;

    // -------------- TOOL STUFF ----------------
    private Pencil PencilTool;
    private Pen PenTool;

    void Start()
    {
        PencilTool = ((Pencil)DrawManager.GetTool(ToolType.Pencil));
        PenTool = ((Pen)DrawManager.GetTool(ToolType.Pen));

        PenTool._updatePenAreaEvent += updatePenArea;

        if (_shockwaveCamera.targetTexture != null)
            _shockwaveCamera.targetTexture.Release();
        RenderTexture temp = new(Screen.width, Screen.height, 24);
        _shockwaveCamera.targetTexture = temp;
        _shockwaveMat.SetTexture("_RenderTexture", temp);

        KingScribble = PlayerVars.instance.gameObject; // Initialize KS, his RigidBody2D, and MainBody trigger collider
        KSrb = KingScribble.GetComponent<Rigidbody2D>();
        EBrb = GetComponent<Rigidbody2D>();
        KSCollider = KingScribble.transform.Find("MainBody").GetComponent<PolygonCollider2D>(); // very iffy code but necessary due to the player being a dynamically spawned object
        prevPosition = transform.position;
        eraserRadius *= transform.localScale.x;

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
            else
            { // isTrigger collider
                Physics2D.IgnoreCollision(col, platformCol);
                Debug.Log($"Ignoring collision between {col.name} and {platformCol.name}");
                Debug.Log("disabling physics isTrigger");
            }
        }

        ChangeState(State.Start);
        //StartCoroutine(ActivateShield());

        /*
        //retrieves ShockwaveMan script from ShockFSTest object
        //retrieves the visual sprite effect for roar
        shockwaveManScript = GetComponentInChildren<ShockwaveMan>();
        if (shockwaveManScript == null)
        {
            Debug.LogError("shockshader script not found");
        }
        //might need to change this not sure if ok for performance reasons
        //used to hide shockwave effect sprite
        */

        spawnerScript = GetComponent<ShockwaveSpawner>();


    }

    // Called only upon entering a state. Good for setting variables and calling functions that do not require FixedUpdate
    // This will be reworked after the semester and will be how states change
    private void ChangeState(State tempState)
    {
        // End the dizzy sound if it's leaving the dizzy state -- TODO rework this, feels bad, sorry - evan
        if ((state == State.Dizzied || state == State.Damaged) && tempState != State.Dizzied && tempState != State.Damaged)
        {
            soundPlayer.EndSound("EraserBoss.Dizzy");
        }

        state = tempState;

        switch (tempState)
        {
            case State.Start:
                StartCoroutine(StartUp());
                EBrb.gravityScale = 0;
                EBrb.drag = 10;
                break;
            case State.Searching:
                timer = 0;
                EBrb.gravityScale = 0; // these values are needed for oscillation
                EBrb.drag = 10;
                break;
            case State.Moving:
                break;
            case State.ChargePrep:
                break;
            case State.WindUp:
                break;
            case State.Charging:
                break;
            case State.ChargeCooldown:
                break;
            case State.SlamPrep:
                EBrb.gravityScale = 0;
                EBrb.drag = 10;
                break;
            case State.Slamming:
                EBrb.gravityScale = 1;
                EBrb.drag = 0;
                break;
            case State.SlamImpact:
                EBrb.sharedMaterial = friction;
                break;
            case State.SlamCooldown:
                EBrb.sharedMaterial = slippery;
                break;
            case State.ShieldActivate:
                EBrb.gravityScale = 0;
                EBrb.drag = 10;
                break;
            case State.ShieldRemove:
                timer = 0;
                EBrb.gravityScale = 1;  // these values are needed for gravity and forces
                EBrb.drag = 0;
                break;
            case State.Roar:
                Invoke(nameof(RoarSound), 10 / 12f); // the time accounts for the delay in the animation
                if (spawnerScript != null)
                {
                    //spawning prefab with delay to sync with roar
                    spawnerScript.Invoke(nameof(spawnerScript.SpawnShockwave), 10 / 12f);
                }
                break;
            case State.Dizzied:
                soundPlayer.PlaySound("EraserBoss.Dizzy", 1, true);
                break;
            case State.Idle:
                EBrb.gravityScale = 0; // these values are needed for oscillation
                EBrb.drag = 10;
                break;
            case State.EndScene:
                EBrb.gravityScale = 0;
                EBrb.drag = 0;
                movingWall.ReturnToStart();
                StartCoroutine(EndScene());
                break;
        }
    }

    void RoarSound()
    {
        soundPlayer.PlaySound("EraserBoss.Roar");
    }

    void FixedUpdate()
    {
        myText.text = state.ToString();

        if (disable) return;

        timer += Time.deltaTime;
        oscillationTimer += Time.deltaTime; // should be placed better in the code for more consistent behavior
        Erase(); // should move Erase() to the designated states eventually!

        switch (state)
        {
            case State.Start:
                Oscillate();
                break;
            case State.Searching:
                anim.Play("EB_Idle");
                SearchForPosition();
                Oscillate();
                break;

            case State.Moving:
                OrientSpriteDirection();
                Hover(destination, baseSpeed);
                // Debug.Log("Moving to: " + destination);
                // Debug.Log("Trasform is: " + transform.position);
                break;

            case State.ChargePrep:
                anim.Play("EB_Idle");

                Hover(destination, baseSpeed); // hover in the average direction of the line
                OrientSpriteDirection();
                //Debug.Log("DISTANCE TO END POINT IS: " + Vector3.Distance(transform.position, destination));

                // when destination reached, start windup
                // explain to me unity why the lowest distance you can get with your movement function is 2.0 like where is the ACCURACY??
                if (Vector3.Distance(transform.position, destination) < 2.5)
                {
                    timer = 0;
                    if (faceRight)
                    {
                        spriteRenderer.flipX = false;
                    }
                    else
                    {
                        spriteRenderer.flipX = true;
                    }

                    ChangeState(State.WindUp);
                }
                if (timer > 3)
                {
                    Debug.LogError("EB could not reach chargePrep position.");
                    timer = 0;
                    ChangeState(State.WindUp);
                }
                break;

            case State.WindUp:
                anim.Play("EB_WindUp");
                Hover(transform.position, 1.0f);
                if (timer >= chargePrepTime)
                {
                    timer = 0;
                    ChangeState(State.Charging);
                }
                break;


            case State.Charging:
                // use the closeset line: determine whether the first or last point of the line is closer and align himself with MoveTo
                anim.Play("EB_Dash");
                OrientSpriteDirection();
                if (!isErasingLine)
                {
                    eraseLineSequence = StartCoroutine(EraseLineSequence(chargeSpeed)); // start coroutine
                }

                // Trying to solve stopping coroutine problem:
                
                // float distance = Vector3.Distance(transform.position, prevPosition);
                // Debug.Log(distance);
                // if (distance < 0.01f)
                // {
                //     Debug.LogWarning("Stopping Coroutine: EB's velocity is too small.");
                //     StopCoroutine(eraseLineSequence);
                // }
                break;

            case State.ChargeCooldown:
                anim.Play("EB_Stop");
                Hover(transform.position, 1.0f);
                if (timer >= chargeCooldownTime)
                {
                    if (spriteRenderer.flipX == true)
                    {
                        spriteRenderer.flipX = false;
                    }
                    timer = 0;
                    ChangeState(State.Searching);
                }
                break;

            case State.Dizzied:
                Hover(transform.position, 1f);
                anim.Play("EB_Stun");
                if (timer >= dizzyTime)
                {
                    ChangeState(State.Searching);
                }
                break;

            case State.Damaged:
                anim.Play("EB_Hurt");
                if (timer >= damageTime)
                {
                    if (!isShielding)
                    {
                        StartCoroutine(ActivateShield());
                    }
                }
                break;

            case State.Roar:
                anim.Play("EB_Roar");
                break;

            case State.ShieldActivate:
                break;

            case State.ShieldRemove:
                anim.Play("EB_Stun");
                break;

            case State.SlamPrep:
                //Debug.Log("State = SlamPrep");
                anim.Play("EB_SlamPrep");
                spriteRenderer.flipX = false;
                if (!isRotated)
                {
                    rotateTween = transform.DORotate(new Vector3(0, 0, -90), rotateTweenTime);
                    isRotated = true;
                }

                // Only track KS pos until 0.5 seconds left in the prep time - evan
                if (timer < 0.8f * slamPrepTime)
                {
                    KSpos = KingScribble.transform.position;
                }

                // Adding the sin function to oscillate, the first 2 is the period
                // Clamp EB's height just below the ceiling so he can oscillate
                Hover(new Vector3(KSpos.x, Mathf.Min(KSpos.y + 22.0f, 22) + Mathf.Sin(Time.time * 1.5f * 2 * Mathf.PI)), baseSpeed); // hover above KS

                if (timer >= slamPrepTime)
                {
                    timer = 0;
                    destination = new Vector3(KSpos.x, -20.0f, KSpos.z); // y value should be below minimum floor
                    ChangeState(State.Slamming);
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
                if (timer >= 1.0f)
                {
                    timer = 0;
                    ChangeState(State.SlamCooldown);
                }
                break;

            case State.SlamCooldown:
                anim.Play("EB_Idle");
                Hover(new Vector3(transform.position.x, -7f, 0f), cooldownSpeed); // -7f is above the ground
                rotateTween = transform.DORotate(new Vector3(0, 0, 0), rotateTweenTime);
                isRotated = false;

                if (timer >= slamCooldownTime)
                {
                    timer = 0;
                    ChangeState(State.Searching);
                }
                break;

            case State.Idle:
                Oscillate();
                anim.Play("EB_Idle");
                break;
            case State.EndScene:
                OrientSpriteDirection();
                //Hover(transform.position + new Vector3(1f,0f,0f), baseSpeed);
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // this if else handles EB's pen object collision
        if (other.CompareTag("Pen") && !other.GetComponent<Line>().deleted)
        { // Get the RigidBody2D and compare its mass 
            GameObject penObj = other.gameObject;
            Rigidbody2D penRB = penObj.GetComponent<Rigidbody2D>();
            if (penRB.mass >= minDamageMass)
            { // Pen obj is big enough
                if (state == State.Charging && !isInvulnerable)
                {
                    Debug.Log("DIZZIED");
                    timer = 0;
                    StopCoroutine(eraseLineSequence); // stop the erasing coroutine
                    ChangeState(State.Dizzied);
                    isErasingLine = false;
                    Destroy(other.gameObject); // destroy pen object
                    other.GetComponent<Line>().deleted = true; // ensures the Line is deleted
                }
                else if (state == State.Dizzied)
                {
                    hitpoints--;
                    Debug.Log("HP at " + hitpoints);
                    Destroy(other.gameObject); // destroy pen object
                    other.GetComponent<Line>().deleted = true;
                    timer = 0;
                    if (hitpoints == 0)
                    {
                        ChangeState(State.EndScene);
                    }
                    else
                    {
                        ChangeState(State.Damaged);
                        Difficulty2();
                    }
                }
            }
            else
            { // Pen obj is too small
                if (state == State.Charging)
                {
                    Destroy(other.gameObject); // destroy pen object
                    other.GetComponent<Line>().deleted = true;
                }
            }
        }

        // handles EB hitting KS
        if (!isKSHit)
        {
            if (other == KSCollider)
            { // Deplete health from KS
                PencilTool.SpendFuel(50);
                Vector3 distance = transform.position - KingScribble.transform.position;
                if (distance.x < 0)
                { // launch right
                    Knockback(new Vector2(1f, 1f), knockbackForce);
                }
                else
                { // launch left 
                    Knockback(new Vector2(-1f, 1f), knockbackForce);
                }
            }
        }

        // this if else handles EB's interaction with his surroundings
        // Ink waterfalls
        if (other.gameObject.layer == LayerMask.NameToLayer("EB_Hurt") && state != State.ShieldRemove)
        {
            // check for left or right pipe
            if (other.gameObject.GetInstanceID() == _leftInk.GetInstanceID())
            {
                if (!isShielding)
                {
                    StartCoroutine(RemoveShield(false));
                }
            }
            if (other.gameObject.GetInstanceID() == _rightInk.GetInstanceID())
            {
                if (!isShielding)
                {
                    StartCoroutine(RemoveShield(true));
                }
            }
        }
        // Stop at the ground when slamming, not at pencil lines
        else if ((other.gameObject.layer == LayerMask.NameToLayer("Ground") || other.gameObject.layer == LayerMask.NameToLayer("Tilemap")) && state == State.Slamming)
        {
            Debug.Log("GROUND DETECTED, pos is: " + transform.position);
            timer = 0;
            isSlamming = false;
            isSlamHit = true;
            ChangeState(State.SlamImpact);
            soundPlayer.PlaySound("EraserBoss.Thud");
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Water") && state == State.Slamming)
        {
            Debug.Log("WATER DETECTED, pos is: " + transform.position);
            EBrb.AddForce(new Vector2(0f, 1f * (slamForce - 10)), ForceMode2D.Impulse);
            timer = 0;
            isSlamming = false;
            isSlamHit = true;
            ChangeState(State.SlamImpact);
            soundPlayer.PlaySound("EraserBoss.Splash");
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Chain") && state == State.Slamming)
        {
            Debug.Log("CHAIN DETECTED, pos is: " + transform.position);
            timer = 0;
            isSlamming = false;
            isSlamHit = true;
            ChangeState(State.SlamImpact);
            soundPlayer.PlaySound("EraserBoss.Thud");
            soundPlayer.PlaySound("EraserBoss.Chain");
        }
        
        // a temporary fix that despawns pen objects until EB's dodge algorithm is implemented
        if (other.gameObject.layer == LayerMask.NameToLayer("PenLines"))
        {
            Destroy(other.gameObject);
        }
    }

    private void OrientSpriteDirection()
    {
        Vector3 direction = transform.position - prevPosition;

        // flip sprite appropriately
        if (direction.x > 0.01)
        {
            spriteRenderer.flipX = false; // face RIGHT
        }
        if (direction.x < -0.01)
        {
            spriteRenderer.flipX = true; // face LEFT
        }

        prevPosition = transform.position;
    }

    void SearchForPosition()
    {
        if (PencilTool.GetLinesFolder() == null) return;

        // If line renderer present, goes for the biggest one OR closest one?
        float closestDistance = 100f;

        foreach (Transform childTransform in PencilTool.GetLinesFolder()) // for each pencil line
        {
            LineRenderer tempLine = childTransform.GetComponent<LineRenderer>();
            if (tempLine.positionCount > minimumLinePoints)
            {
                // for each first and last point in the pencil line find which is the closest to EB
                float pointDistanceFirst = Vector3.Distance(transform.position, tempLine.GetPosition(0) + tempLine.transform.position); // first point in line
                float pointDistanceLast = Vector3.Distance(transform.position, tempLine.GetPosition(tempLine.positionCount - 1) + tempLine.transform.position); // last point in line


                if (pointDistanceFirst < closestDistance)
                {
                    closestDistance = pointDistanceFirst;
                    firstIsClosest = true;
                    closestLine = tempLine;
                }
                if (pointDistanceLast < closestDistance)
                {
                    closestDistance = pointDistanceLast;
                    firstIsClosest = false;
                    closestLine = tempLine;
                }
            }
        }

        if (closestLine != null)
        { // target is the closest line
            targetLine = closestLine;
            setLineData();
        }

        if (timer >= searchTime)
        {
            timer = 0;
            if (closestLine != null)
            {
                ChangeState(State.ChargePrep);
            }
            else
            {
                ChangeState(State.SlamPrep);
            }
        }
    }

    void Hover(Vector3 destination, float speed)
    {
        float step = speed * Time.deltaTime; // Calculate the maxDistanceDelta based on the distance
        EBrb.MovePosition(Vector2.MoveTowards(transform.position, destination, step));
    }

    void Slam()
    {
        if (!isSlamming)
        {
            //Debug.Log("APPLYING SLAM FORCE");
            EBrb.AddForce(new Vector2(0f, -1f * slamForce), ForceMode2D.Impulse);
            isSlamming = true;
        }
    }

    // Takes into account EB's circlecast colliders
    void Erase()
    {
        EraserFunctions.Erase(bounds1.transform.position, eraserRadius, true, PencilTool.GetLinesFolder());
        EraserFunctions.Erase(bounds2.transform.position, eraserRadius, true, PencilTool.GetLinesFolder());
        EraserFunctions.Erase(bounds3.transform.position, eraserRadius, true, PencilTool.GetLinesFolder());
    }


    // used to avoid null references to a line that will be erased
    private void setLineData()
    {
        numPoints = targetLine.positionCount;
        targetPosition = targetLine.transform.position;
        tempArray = new Vector3[numPoints];
        targetLine.GetPositions(tempArray); // get the positions into the array
        if (firstIsClosest)
        {
            lineUp = (targetLine.GetPosition(0) - targetLine.GetPosition(1)).normalized;  // this line sometimes bugs
            destination = targetLine.GetPosition(0) + targetLine.transform.position + (lineUp * 4); // position 0 in line renderer
        }
        else
        {
            lineUp = (targetLine.GetPosition(numPoints - 1) - targetLine.GetPosition(numPoints - 2)).normalized;
            destination = targetLine.GetPosition(numPoints - 1) + targetLine.transform.position + (lineUp * 4); // position 0 in line renderer
        }

        // orient EB to face the right direction
        if (lineUp.x < 0)
        {
            faceRight = true;
        }
        else
        {
            faceRight = false;
        }
    }


    private IEnumerator EraseLineSequence(float speed)
    {
        isErasingLine = true;
        float step; // calculate the maxDistanceDelta based on the distance
        int mult = 3; // multipler if points need to be iterated not one by one

        if (firstIsClosest)
        {
            for (int i = 0; i < numPoints;)
            { // for each point in the pencil line move
                Vector3 point = tempArray[i] + targetPosition; // the destination
                step = speed * Time.fixedDeltaTime;

                EBrb.MovePosition(Vector2.MoveTowards(transform.position, point, step));

                //END POINT IS: " + Vector3.Distance(transform.position, point));
                if (Vector3.Distance(transform.position, point) < 2.5f)
                {  // was > 0.01f
                    i += mult;
                }
                yield return new WaitForSeconds(0.01f); // wait for a bit... i think
            }
        }
        else
        {
            for (int i = numPoints - 1; i > -1;)
            { // for each point in the pencil line move
                Vector3 point = tempArray[i] + targetPosition; // the destination
                step = speed * Time.fixedDeltaTime;
                EBrb.MovePosition(Vector2.MoveTowards(transform.position, point, step));

                //Debug.Log("END POINT IS: " + Vector3.Distance(transform.position, point));
                if (Vector3.Distance(transform.position, point) < 2.5f)
                {  // was > 0.01f
                    //Debug.LogWarning("increment i = " + i);
                    i -= mult;
                }
                yield return new WaitForSeconds(0.01f); // wait for a bit... i think
            }
        }

        //Debug.Log("EXITED FOR LOOP " + Vector3.Distance(transform.position, tempArray[numPoints - 1] + targetPosition));
        //if(Vector3.Distance(transform.position, tempArray[numPoints - 1] + targetPosition) < 2 || timer > 5) {
        isErasingLine = false;
        timer = 0;
        ChangeState(State.ChargeCooldown);
    }

    // Knocks back the player if they are not in the hit cooldown period
    private void Knockback(Vector2 knockbackDirection, float force)
    {
        if (KSrb != null && !isKSHit)
        {
            Debug.Log("KNOCKING BACK");
            PlayerController.instance.Hurt();
            KSrb.AddForce(knockbackDirection * force, ForceMode2D.Impulse); // Use Impulse or VelocityChange
            isKSHit = true;
            print("state: " + state);
            if (isSlamHit)
            {
                StartCoroutine(StunPlayer(KSStunTime));
            }
            else
            {
                StartCoroutine(HitCooldown(KSHitCooldown));
            }

        }
    }


    // Used for moving wall cutscene, no walking left outside the fight!
    public void StunPlayerFor(float duration)
    {
        StartCoroutine(StunPlayer(duration));
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

    private IEnumerator HitCooldown(float duration)
    { // Hit Cooldown when KS gets hit
        yield return new WaitForSeconds(duration);
        isKSHit = false;
        isSlamHit = false;
    }

    // Despawns all pen objects in scene and knocks back KS off platform
    private IEnumerator ActivateShield()
    {
        ChangeState(State.ShieldActivate);
        Debug.Log("ACTIVATING SHIELD");
        yield return new WaitForSeconds(0.01f);

        ChangeState(State.Roar);
        isShielding = true;
        yield return new WaitForSeconds(1.0f);

        isInvulnerable = true;
        shieldSprite.SetActive(true);
        eraserBossEvent.ActivateButton();
        Debug.Log("ACTIVATING BUTTON");
        DespawnAllPenObj();

        yield return new WaitForSeconds(2.0f);

        ChangeState(State.Searching);
        isShielding = false;
    }

    private void updatePenArea(float area)
    {
        totalPenArea += area;
        Debug.Log("area: " + area);
        if (totalPenArea > maxPenArea)
        {
            DespawnAllPenObj();
        }
        totalPenArea = 0;
    }

    private void DespawnAllPenObj()
    {
        //Debug.Log("DESPAWNING PEN OBJS");
        if (PenTool.GetLinesFolder() == null) return;
        foreach (Transform childTransform in PenTool.GetLinesFolder())
        {
            if (childTransform.gameObject.layer == 7) // Only runs on spawned pen objects
                StartCoroutine(DespawnPenObj(childTransform));
        }
    }

    private IEnumerator DespawnPenObj(Transform pen)
    {
        yield return new WaitForSeconds(.25f); // wait for MaterialPropertyBlock to load
        // play pen obj despawn warning animation
        // this code kinda sucks :(
        if (pen != null)
        {
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
            if (polyRend != null && !pen.GetComponent<Line>().deleted)
            {
                polyRend.SetPropertyBlock(matBlockOpacity);
                tempLine.startColor = opacity;
                tempLine.endColor = opacity;
                yield return new WaitForSeconds(.5f);
            }
            if (polyRend != null && !pen.GetComponent<Line>().deleted)
            {
                polyRend.SetPropertyBlock(matBlockOG);
                tempLine.startColor = original;
                tempLine.endColor = original;
                yield return new WaitForSeconds(.5f);
            }
            if (polyRend != null && !pen.GetComponent<Line>().deleted)
            {
                polyRend.SetPropertyBlock(matBlockOpacity);
                tempLine.startColor = opacity;
                tempLine.endColor = opacity;
                yield return new WaitForSeconds(.5f);
            }
            if (polyRend != null && !pen.GetComponent<Line>().deleted)
            {
                polyRend.SetPropertyBlock(matBlockOG);
                tempLine.startColor = original;
                tempLine.endColor = original;
                yield return new WaitForSeconds(.5f);
            }
            if (polyRend != null && !pen.GetComponent<Line>().deleted)
            {
                pen.GetComponent<Line>().deleted = true;
                Destroy(pen.gameObject);
            }
        }
    }

    private void DespawnAllPencilObj()
    {
        if (PencilTool.GetLinesFolder() == null) return;
        foreach (Transform childTransform in PencilTool.GetLinesFolder())
        {
            StartCoroutine(DespawnPencilObj(childTransform));
        }
    }


    private IEnumerator DespawnPencilObj(Transform pencil)
    {
        yield return new WaitForSeconds(.25f); // so we line up with the pen object delay
        // this code kinda sucks :(
        if (pencil != null)
        {
            LineRenderer tempLine = pencil.GetComponent<LineRenderer>();
            Color original = tempLine.startColor;
            Color opacity = new Color(tempLine.startColor.r, tempLine.startColor.g, tempLine.startColor.b, 0.5f);

            // toggle opacities
            // ADD NULL CHECK cuz the line can be erased
            if (tempLine != null && !tempLine.GetComponent<Line>().deleted)
            {
                tempLine.startColor = opacity;
                tempLine.endColor = opacity;
                yield return new WaitForSeconds(.5f);
            }
            if (tempLine != null && !tempLine.GetComponent<Line>().deleted)
            {
                tempLine.startColor = original;
                tempLine.endColor = original;
                yield return new WaitForSeconds(.5f);
            }
            if (tempLine != null && !tempLine.GetComponent<Line>().deleted)
            {
                tempLine.startColor = opacity;
                tempLine.endColor = opacity;
                yield return new WaitForSeconds(.5f);
            }
            if (tempLine != null && !tempLine.GetComponent<Line>().deleted)
            {
                tempLine.startColor = original;
                tempLine.endColor = original;
                yield return new WaitForSeconds(.5f);
            }
            if (tempLine != null && !tempLine.GetComponent<Line>().deleted)
            {
                pencil.GetComponent<Line>().deleted = true;
                Destroy(pencil.gameObject);
                PencilTool.AddFuel(tempLine.positionCount); // Give player back their health
            }
        }
    }

    // Happens when the player pushes the button and EB gets hit with ink falling
    private IEnumerator RemoveShield(bool isRight)
    {
        StopCoroutine(eraseLineSequence);
        isErasingLine = false;
        ChangeState(State.ShieldRemove);
        Debug.Log("DEACTIVATING SHIELD");
        isShielding = true;
        isInvulnerable = false;
        shieldSprite.SetActive(false);

        yield return new WaitForSeconds(0.5f);
        ChangeState(State.Roar);
        yield return new WaitForSeconds(0.5f);

        EBrb.AddForce(new Vector2(0f, 1f * slamForce), ForceMode2D.Impulse); // break pipe
        yield return new WaitForSeconds(0.35f);

        if (isRight)
        {
            eraserBossEvent.DeactivateRight(); // so ink cannot flow again from it
        }
        else
        {
            eraserBossEvent.DeactivateLeft();
        }

        rotateTween = transform.DORotate(new Vector3(0, 0, -90), rotateTweenTime);
        isRotated = true;
        yield return new WaitForSeconds(1.0f);
        EBrb.sharedMaterial = friction;

        EBrb.AddForce(new Vector2(0f, -1f * slamForce), ForceMode2D.Impulse); // slamming
        Debug.Log("SHIELD SLAM FORCE ADDED");
        yield return new WaitForSeconds(.75f);

        // SLOW THE TIMEEEE
        Debug.Log("BREAKING CHAIN");
        
        if (isRight) { // break chain
            BreakRightChain();
        }
        else {
            BreakLeftChain();
        }

        ChangeState(State.Idle); // this is so the Roar animation plays again!
        yield return new WaitForSeconds(1.0f);
        rotateTween = transform.DORotate(new Vector3(0, 0, 0), rotateTweenTime);
        EBrb.sharedMaterial = slippery;
        isRotated = false;
        EBrb.AddForce(new Vector2(0f, 1f * 2000), ForceMode2D.Impulse);  // add a force upward so he doesnt drown

        ChangeState(State.Roar);
        yield return new WaitForSeconds(10 / 12f);
        eraserBossEvent.DeactivateButton();
        DespawnAllPenObj();
        DespawnAllPencilObj();

        // Knockback KS to a wall!
        Vector3 distance = transform.position - KingScribble.transform.position;
        if (distance.x < 0)
        { // launch right
            Knockback(new Vector2(1f, .1f), roarForce);
        }
        else
        { // launch left 
            Knockback(new Vector2(-1f, .1f), roarForce);
        }
        yield return new WaitForSeconds(2.0f);

        Debug.Log("EXITING REMOVESHIELD");
        isShielding = false;
        ChangeState(State.Searching);
    }

    private void BreakLeftChain()
    {
        leftChainL.GetComponent<BreakableChainLink>().Break();
        leftChainR.GetComponent<BreakableChainLink>().Break();
        soundPlayer.PlaySound("EraserBoss.ChainBreak");
    }

    private void BreakRightChain()
    {
        rightChainL.GetComponent<BreakableChainLink>().Break();
        rightChainR.GetComponent<BreakableChainLink>().Break();
        soundPlayer.PlaySound("EraserBoss.ChainBreak");
    }

    // increase the speeds and initiates shield:
    private void Difficulty2()
    {
        Debug.Log("INCREASING DIFFICULTY");
        baseSpeed += 20;
        chargeSpeed += 20;
    }

    // simulates an oscillating hover movement (not perfectly but close enough for boss fight purposes! :p)
    private void Oscillate()
    {
        if (oscillationTimer % swap > (swap / 2))
        {
            EBrb.AddForce(new Vector2(0f, oscillation), ForceMode2D.Force);
        }
        else
        {
            EBrb.AddForce(new Vector2(0f, -oscillation), ForceMode2D.Force);
        }
    }

    // unbinds delegate upon destroying the eraser boss -- this is good practice!! - evan
    private void OnDestroy()
    {
        PenTool._updatePenAreaEvent -= updatePenArea;
    }

    // Cut scene that shows EB roaring
    private IEnumerator StartUp()
    {
        // despawn existing pencil and pen objects outside the arena
        yield return new WaitForSeconds(1.0f);
        ChangeState(State.Roar);
        yield return new WaitForSeconds(0.5f);
        DespawnAllPencilObj();
        DespawnAllPenObj();
        yield return new WaitForSeconds(2.5f);
        ChangeState(State.Searching);
    }

    private IEnumerator EndScene()
    {
        baseSpeed = 25f;
        anim.Play("EB_Stun");
        yield return new WaitForSeconds(2.0f);

        destination = new Vector3(transform.position.x, -9.25f, 0); // front of door coords
        ChangeState(State.Moving);
        yield return new WaitForSeconds(1.0f);

        destination = new Vector3(120, -9.25f, 0);
        yield return new WaitForSeconds(5.0f);
        gameObject.SetActive(false); // bye bye :D

    }
    // returns a point in bounds of the arena
    // arena bounds 3 < x 72.3 AND -21.5 < y < 11
    private Vector3 checkBounds(Vector3 point)
    {
        return point;
    }
}