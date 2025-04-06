using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

public class EraserBossAI : MonoBehaviour
{    
    private enum State {
        Searching, // search for a position
        Moving,
        ChargePrep,
        Charging, // for lines
        Dizzied,
        Damaged,
        SlamPrep, // hovering above KS before slam
        Slamming, // for KS on ground
        Cooldown // used for Slam Cooldown and Charge Cooldown
    }
    [SerializeField] private bool disable = false;

    [SerializeField] private GameObject PencilLinesFolder; // Where pencil lines are stored in hierarchy
    [SerializeField] public float baseSpeed = 30f; // Movement speed
    [SerializeField] public float cooldownSpeed = 10f; // slower speed for cooldown to mimic tiredness
    [SerializeField] public float eraserRadius = 1f; // Space that will be erased
    [SerializeField] public float knockbackForce = 30f; // upon KS hitting EB

    private State state;
    public GameObject KingScribble;
    private Transform target; // current target's transform: this is either KS or a LineRenderer in PencilLinesFolder
    private Vector3 destination; // position for where to move
    private SpriteRenderer spriteRenderer; // for animation
    private GameObject bounds1; // for Erase circle cast, requires 2 circle colliders
    private GameObject bounds2;
    private Collider2D KSCollider;
    [SerializeField] private float tweenTime = .3f; // the amount of time for the slam and charge tween to complete
    private float timer = 0.0f; // used for cooldowns
    private float searchTime = 5.0f; // variables ending in "Time" relate to the timer
    private float slamCooldownTime = 2.0f;
    private float slamPrepTime = 2.0f;
    private float KSHitCooldown = 2.0f; // cooldown for how long until KS can be hit again
    private bool isErasingLine = false; // booleans for states that are not independent enough for the state machine
    private bool isTweening = false;
    private bool isRotated = false;
    private bool isKSHit = false;

    private Tween slamTween;
    
    private Rigidbody2D KSrb; // King Scribble's Rigidbody
    private CapsuleCollider2D physicalCollider; // EB's collider with physics

    void Start() {
        KingScribble = PlayerVars.instance.gameObject;
        KSCollider = KingScribble.transform.Find("MainBody/Collider").GetComponent<CircleCollider2D>(); // very iffy code
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        bounds1 = transform.Find("Bounds1").gameObject;
        bounds2 = transform.Find("Bounds2").gameObject;
        disable = true;
        KSrb = KingScribble.GetComponent<Rigidbody2D>();

        foreach (CapsuleCollider2D col in GetComponents<CapsuleCollider2D>())
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
        if(disable) { return;}
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
                    EraseLineSequence();
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
                    destination += new Vector3(0, -10, 0);
                    state = State.Slamming;
                }
                break;

            case State.Slamming:
                Slam(destination, tweenTime);
                EraserFunctions.Erase(bounds1.transform.position, eraserRadius, false, PencilLinesFolder);
                EraserFunctions.Erase(bounds2.transform.position, eraserRadius, false, PencilLinesFolder);
                break;

            case State.Cooldown:
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
        // Stop at the ground when slamming, not at pencil lines
        if (other.CompareTag("Ground") && other.gameObject.layer != LayerMask.NameToLayer("Lines") && state == State.Slamming) {
            // Debug.Log("GROUND DETECTED");
            slamTween.Kill();
            timer = 0;
            spriteRenderer.color = Color.yellow;
            isTweening = false;
            state = State.Cooldown;
        }
        
        if (other == KSCollider) { // Deplete health from KS
            KingScribble.transform.position += new Vector3(-10f, 0f, 0f);

            /* WORK IN PROGRESS
            if(!isKSHit) {
                physicalCollider.enabled = false;
                PlayerVars.instance.SpendDoodleFuel(50);
                Knockback();
            }
            */
        }
        // Destroy pen obj is mass is big enough
        if (other.gameObject.layer == LayerMask.NameToLayer("Lines")) {
            // WIP
        }
    }

    void SearchForPosition() {
        // If line renderer present, goes for the biggest one
        LineRenderer closestLine = null;
        bool closestLineFound = false;
        float KSDistance = Vector3.Distance(this.transform.position, KingScribble.transform.position);
        
        foreach (Transform childTransform in PencilLinesFolder.transform)
        {
            LineRenderer tempLine = childTransform.GetComponent<LineRenderer>();
            if(tempLine.positionCount != 0) {
                float lineDistance = Vector3.Distance(this.transform.position, tempLine.GetPosition(0));
                if(lineDistance < KSDistance) {
                    closestLine = tempLine;
                    closestLineFound = true;
                }
            }
        }

        if(closestLineFound) { // target is the closest line
            target = closestLine.transform; // closestLine.GetPosition(0) + closestLine.transform.position;
        }
        else {
            target = KingScribble.transform;
        }

        if(timer >= searchTime){
            timer = 0;
            spriteRenderer.color = Color.yellow;
            if(closestLineFound) { state = State.ChargePrep; }
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

    void EraseLineSequence() {
        //isErasingLine = true;
        state = State.Cooldown;

    }

    private void Knockback() {
        if (KSrb != null && !isKSHit) {
            Debug.Log("KNOCKING BACK");
            Vector2 knockbackDirection = new Vector2(-1f, 1f);
            KSrb.AddForce(knockbackDirection * 20.0f, ForceMode2D.Impulse); // Use Impulse or VelocityChange
            isKSHit = true;
            StartCoroutine(StunPlayer(2.0f));
        }
        else { Debug.Log("RIGIDBODY NOT FOUND"); }
    }

    private IEnumerator StunPlayer(float duration)
    {
        KSrb.constraints = RigidbodyConstraints2D.FreezeAll; // freeze movement
        yield return new WaitForSeconds(duration);
        KSrb.constraints = RigidbodyConstraints2D.None; // unfreeze
        Debug.Log("UNFREEZE");
        yield return new WaitForSeconds(5.0f);
        Debug.Log("KNOCKBACK ENABLED");
        isKSHit = false;
    }
}