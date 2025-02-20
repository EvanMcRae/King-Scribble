using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    private Rigidbody2D rb;
    private CircleCollider2D cldr;
    [SerializeField] private float jumpForce = 800f;
    private float jumpTime, lastOnLand, lastLandHeight, timeSinceJump, timeSinceJumpPressed, beenOnLand, fallTime,  jumpSpeedMultiplier, sprintSpeedMultiplier;
    [SerializeField] private float maxSprintSpeedMultiplier;
    [SerializeField] private float slopeCheckDistance;
    [SerializeField] private float maxSlopeAngle;
    private float slopeDownAngle;
    private float slopeSideAngle;
    private float lastSlopeAngle;
    private Vector2 slopeNormalPerp;
    public bool isOnSlope, canWalkOnSlope;
    public PhysicsMaterial2D slippery, friction;
    private float moveX;
    public bool isJumping = false, isSprinting = false, isRoofed = false, isDead = false, isFalling = false;
    private bool releasedJumpSinceJump = false, needToHalfVelocity = false;
    public bool facingRight
    {
        get
        {
            return transform.localScale.x > 0;
        }
        set
        {
            int neg = 1;
            if (value)
                neg *= -1;

            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * neg, transform.localScale.y, transform.localScale.z);
        }
    }
    private Vector3 velocity = Vector3.zero;
    [SerializeField] private float speed;
    [SerializeField] private float movementSmoothing;

    private float calculatedSpeed;
    public bool holdingJump;

    public bool isGrounded = false;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private PolygonCollider2D groundCheck, roofCheck;
    [SerializeField] private float groundedRadius, roofedRadius;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cldr = GetComponent<CircleCollider2D>();

        // Singleton
        if (this != instance && instance != null)
        {
            Destroy(gameObject);
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        timeSinceJumpPressed = 0.2f;
        jumpSpeedMultiplier = 1f;
        sprintSpeedMultiplier = 1f;
        jumpTime = 0f;
        facingRight = true;
    }

    // Update is called once per frame
    void Update()
    {
        moveX = Input.GetAxisRaw("Horizontal");
        
        Jump();

        if (timeSinceJumpPressed < 1f)
            timeSinceJumpPressed += Time.deltaTime;
        if (timeSinceJump < 1f)
            timeSinceJump += Time.deltaTime;

        if (Input.GetButtonDown("Sprint"))
        {
            isSprinting = true;
            sprintSpeedMultiplier = maxSprintSpeedMultiplier;
        }

        if (Input.GetButtonUp("Sprint"))
        {
            isSprinting = false;
            sprintSpeedMultiplier = 1f;
        }
    }

    void FixedUpdate()
    {
        // TODO check for walls here?
        
        if ((moveX < 0 && facingRight) || (moveX > 0 && !facingRight))
        {
            facingRight = !facingRight;
            transform.localScale = new Vector3(-1 * transform.localScale.x, 1, 1);
        }

        // calculate speed
        calculatedSpeed = speed * Mathf.Min(jumpSpeedMultiplier * sprintSpeedMultiplier, 2.0f);

        // calculate target velocity
        Vector3 targetVelocity = new Vector2(isDead ? 0 : moveX * calculatedSpeed, rb.velocity.y);

        // check for ground/roof
        GroundCheck();
        RoofCheck();

        // sloped movement
        SlopeCheck();
        if (isOnSlope && isGrounded && !isJumping && canWalkOnSlope)
        {
            targetVelocity.Set(isDead ? 0 : moveX * calculatedSpeed * -slopeNormalPerp.x, moveX * speed * -slopeNormalPerp.y, 0.0f);
        }

        // apply velocity, dampening between current and target
        if (moveX == 0.0 && rb.velocity.x != 0.0f)
        {
            if (canWalkOnSlope)
                cldr.sharedMaterial = friction;
            rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref velocity, movementSmoothing * 2.5f);
        }
        else
        {
            cldr.sharedMaterial = slippery;
            rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref velocity, movementSmoothing);
        }


        // fall detection
        if (beenOnLand >= 0.1f && !isJumping && !isGrounded && !isFalling)
        {
            isFalling = true;
        }
        if (isFalling)
        {
            fallTime += Time.fixedDeltaTime;
            if (isGrounded && fallTime > 0.1f)
            {
                isFalling = false;
                fallTime = 0.0f;
            }
        }

        // land detection
        if (!isGrounded)
        {
            beenOnLand = 0f;
        }
        else
        {
            if (beenOnLand < 5f)
                beenOnLand += Time.fixedDeltaTime;
            if (!(rb.velocity.y > 0f && !isOnSlope) && isJumping)
            {
                jumpSpeedMultiplier = 1f;
                isJumping = false;
                jumpTime = 0f;
                releasedJumpSinceJump = false;
            }
        }

        // hold jump distance extentions
        if (isJumping && !releasedJumpSinceJump)
        {
            jumpTime += Time.fixedDeltaTime;
            jumpSpeedMultiplier = 1f + 2f / (10f * jumpTime + 4f);
            if (holdingJump)
            {
                jumpSpeedMultiplier *= 1.25f;
                rb.AddForce(new Vector2(0f, rb.mass * jumpForce / 400f / jumpTime));
            }
        }
        else
        {
            jumpSpeedMultiplier = Mathf.Lerp(jumpSpeedMultiplier, 1, 0.3f);
        }
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded && !(rb.velocity.y > 0f) && isJumping)
            {
                jumpSpeedMultiplier = 1f;
                isJumping = false;
                jumpTime = 0f;
            }
            timeSinceJumpPressed = 0.0f;

            holdingJump = true;
        }

        // incorporates coyote time and input buffering
        float coyoteTimeThreshold = 0.1f;
        bool coyoteTime = lastOnLand < 0.2f && transform.position.y < lastLandHeight - coyoteTimeThreshold;

        if (timeSinceJumpPressed < 0.2f && (isGrounded || coyoteTime) && !isRoofed && !isJumping)
        {
            if (isOnSlope && slopeDownAngle > maxSlopeAngle)
            {
                return;
            }

            // Add a vertical force to the player
            isGrounded = false;
            isJumping = true;
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(new Vector2(0f, jumpForce * rb.mass * (holdingJump ? 1 : 0.7f))); // force added during a jump
            timeSinceJump = 0.0f;
            if (holdingJump && releasedJumpSinceJump)
            {
                releasedJumpSinceJump = false;
            }
        }

        if (needToHalfVelocity && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y / 2);
            needToHalfVelocity = false;
        }

        if (Input.GetButtonUp("Jump"))
        {
            if (holdingJump && isJumping && rb.velocity.y >= 0)
            {
                if (rb.velocity.y > 0)
                {
                    rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y / 2);
                }
                else
                {
                    needToHalfVelocity = true;
                }
                releasedJumpSinceJump = true;
            }
            holdingJump = false;
            jumpTime = 0;
        }
    }

    void GroundCheck()
    {
        lastOnLand = Mathf.Clamp(lastOnLand + Time.fixedDeltaTime, 0, 20f);

        isGrounded = false;
        if (groundCheck.IsTouchingLayers(whatIsGround))
        {
            isGrounded = true;
            lastOnLand = 0f;
            lastLandHeight = transform.position.y;
        }
    }

    void RoofCheck()
    {
        isRoofed = false;
        if (roofCheck.IsTouchingLayers(whatIsGround))
        {
            isRoofed = true;
        }
    }

    // Referenced: https://www.youtube.com/watch?v=QPiZSTEuZnw
    void SlopeCheck()
    {
        Vector2 checkPos = transform.position - (Vector3)new Vector2(0.0f, cldr.radius / 2);
        SlopeCheckHorizontal(checkPos);
        SlopeCheckVertical(checkPos);
    }

    void SlopeCheckHorizontal(Vector2 checkPos)
    {
        RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPos, transform.right, slopeCheckDistance, whatIsGround);
        RaycastHit2D slopeHitBack = Physics2D.Raycast(checkPos, -transform.right, slopeCheckDistance, whatIsGround);

        if (slopeHitFront)
        {
            isOnSlope = true;

            slopeSideAngle = Vector2.Angle(slopeHitFront.normal, Vector2.up);

        }
        else if (slopeHitBack)
        {
            isOnSlope = true;

            slopeSideAngle = Vector2.Angle(slopeHitBack.normal, Vector2.up);
        }
        else
        {
            slopeSideAngle = 0.0f;
            isOnSlope = false;
        }
    }

    void SlopeCheckVertical(Vector2 checkPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, slopeCheckDistance, whatIsGround);

        if (hit)
        {
            slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;
            slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeDownAngle != lastSlopeAngle)
            {
                isOnSlope = true;
            }
            lastSlopeAngle = slopeDownAngle;
            Debug.DrawRay(hit.point, slopeNormalPerp, Color.blue);
            Debug.DrawRay(hit.point, hit.normal, Color.green);
        }

        canWalkOnSlope = !(slopeDownAngle > maxSlopeAngle || slopeSideAngle > maxSlopeAngle);
    }
}