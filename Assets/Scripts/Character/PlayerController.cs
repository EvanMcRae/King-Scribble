using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    private PlayerVars vars;
    private Rigidbody2D rb;
    [SerializeField] private Animator anim;
    [SerializeField] private CircleCollider2D[] cldrs;
    [SerializeField] private GameObject mainBody; 
    [SerializeField] private float jumpForce = 800f;
    private float jumpTime, lastOnLand, lastLandHeight, timeSinceJump, timeSinceJumpPressed, beenOnLand, fallTime,  jumpSpeedMultiplier, sprintSpeedMultiplier;
    [SerializeField] private float maxSprintSpeedMultiplier;
    [SerializeField] private float slopeCheckDistance;
    [SerializeField] private float maxSlopeAngle;
    private float slopeDownAngle;
    private float slopeSideAngle;
    private float lastSlopeAngle;
    private Vector2 slopeNormalPerp;
    private bool isOnSlope, canWalkOnSlope;
    public PhysicsMaterial2D slippery, friction;
    private float moveX;
    private bool isJumping = false, isSprinting = false, isRoofed = false, isFalling = false;
    public float levelZoom;
    private bool isSprintMoving = false;
    private bool releasedJumpSinceJump = false, needToCutJump = false;
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
    private bool holdingJump;
    private bool isGrounded = false;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private PolygonCollider2D groundCheck, roofCheck, landCheck;
    [SerializeField] private float groundedRadius, roofedRadius;
    public CinemachineVirtualCamera virtualCamera;
    private float realVelocity;
    private Vector3 lastPosition;
    [SerializeField] private Transform checkPos;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        instance = this;
        
        vars = GetComponent<PlayerVars>();
        timeSinceJumpPressed = 0.2f;
        jumpSpeedMultiplier = 1f;
        sprintSpeedMultiplier = 1f;
        jumpTime = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.paused)
        {
            // Check for releasing jump during pause
            JumpCutCheck();
            return;
        }
        if (!GameManager.canMove)
        {
            moveX = 0;
            JumpCutCheck();
            if (isSprintMoving)
            {
                DOTween.To(() => virtualCamera.m_Lens.OrthographicSize, x => virtualCamera.m_Lens.OrthographicSize = x, levelZoom, 1f);
                isSprintMoving = false;
                transform.DOMove(new Vector3(0, 0, 1), 1).SetEase(Ease.OutElastic);
            }
            isSprinting = false;
            sprintSpeedMultiplier = 1f;
        }
        
        anim.SetBool("isJumping", isJumping);
        anim.SetBool("isFalling", isFalling);
        anim.SetBool("isSprinting", isSprinting);
        anim.SetBool("isGrounded", isGrounded);

        if (vars.isDead || !GameManager.canMove) return;

        moveX = Input.GetAxisRaw("Horizontal");
        anim.SetBool("isMoving", moveX != 0 && Mathf.Abs(realVelocity) >= 0.01f);

        Jump();

        if (timeSinceJumpPressed < 1f)
            timeSinceJumpPressed += Time.deltaTime;
        if (timeSinceJump < 1f)
            timeSinceJump += Time.deltaTime;

        if (Input.GetButton("Sprint"))
        {
            isSprinting = true;
            sprintSpeedMultiplier = maxSprintSpeedMultiplier;
            if (moveX != 0 && Mathf.Abs(realVelocity) >= 0.01f && !isSprintMoving && virtualCamera != null)
            {
                DOTween.To(() => virtualCamera.m_Lens.OrthographicSize, x => virtualCamera.m_Lens.OrthographicSize = x, levelZoom + 0.5f, 1f);
                isSprintMoving = true;
            }
            else if ((moveX == 0 || Mathf.Abs(realVelocity) < 0.01f) && isSprintMoving && virtualCamera != null)
            {
                DOTween.To(() => virtualCamera.m_Lens.OrthographicSize, x => virtualCamera.m_Lens.OrthographicSize = x, levelZoom, 1f);
                isSprintMoving = false;
            }
        }

        if (Input.GetButtonUp("Sprint"))
        {
            if (isSprintMoving)
            {
                DOTween.To(() => virtualCamera.m_Lens.OrthographicSize, x => virtualCamera.m_Lens.OrthographicSize = x, levelZoom, 1f);
                isSprintMoving = false;
            }
            isSprinting = false;
            sprintSpeedMultiplier = 1f;
        }
    }

    public void KillTweens()
    {
        DOTween.KillAll();
    }

    void FixedUpdate()
    {
        realVelocity = (transform.position.x - lastPosition.x) / Time.fixedDeltaTime;
        lastPosition = transform.position;

        if ((moveX < 0 && facingRight) || (moveX > 0 && !facingRight))
        {
            facingRight = !facingRight;
            transform.localScale = new Vector3(-1 * transform.localScale.x, 1, 1);
        }

        // calculate speed
        calculatedSpeed = speed * Mathf.Min(jumpSpeedMultiplier * sprintSpeedMultiplier, 2.0f);

        // calculate target velocity
        Vector3 targetVelocity = new Vector2(vars.isDead ? 0 : moveX * calculatedSpeed, rb.velocity.y);

        // check for ground/roof
        GroundCheck();
        // TODO disabled for now, feels bad
        // RoofCheck(); 

        // sloped movement
        SlopeCheck();
        if (isOnSlope && isGrounded && !isJumping && canWalkOnSlope)
        {
            targetVelocity.Set(vars.isDead ? 0 : moveX * calculatedSpeed * -slopeNormalPerp.x, moveX * speed * -slopeNormalPerp.y, 0.0f);
        }

        // apply velocity, dampening between current and target
        if (moveX == 0.0 && rb.velocity.x != 0.0f)
        {
            if (canWalkOnSlope || !isOnSlope)
            {
                foreach (CircleCollider2D cldr in cldrs)
                {
                    cldr.sharedMaterial = friction;
                }
            }
            rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref velocity, movementSmoothing * 5f);
        }
        else
        {
            foreach (CircleCollider2D cldr in cldrs)
            {
                cldr.sharedMaterial = slippery;
            }
            rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref velocity, movementSmoothing);
        }

        // fall detection
        if (lastOnLand >= 0.1f && !isJumping && !isGrounded && !isFalling)
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
            if (isJumping)
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
            jumpSpeedMultiplier = 0.75f + 1f / (10f * jumpTime + 4f);
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
            if (isGrounded && isJumping)
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
            anim.SetTrigger("justJumped");
            // TODO disabled, would reject jumps if on too steep of a slope
            // if (isOnSlope && slopeDownAngle > maxSlopeAngle) return;

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

        if (needToCutJump && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y / 2);
            needToCutJump = false;
        }

        JumpCutCheck();
    }

    void JumpCutCheck()
    {
        if (Input.GetButtonUp("Jump"))
        {
            if (holdingJump && isJumping && rb.velocity.y >= 0)
            {
                if (rb.velocity.y > 0)
                {
                    rb.velocity = new Vector2(rb.velocity.x, 0.75f * rb.velocity.y);
                }
                else
                {
                    needToCutJump = true;
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
            anim.SetBool("isLanding", false);
            anim.ResetTrigger("justJumped");
            lastLandHeight = transform.position.y;
        }
        else if (rb.velocity.y < 0)
        {
            landCheck.transform.localPosition = groundCheck.transform.localPosition + 1.25f * Vector3.down;
            if (landCheck.IsTouchingLayers(whatIsGround))
            {
                anim.SetBool("isLanding", true);
            }
        }
        else
        {
            anim.SetBool("isLanding", false);
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
        SlopeCheckHorizontal(checkPos.position);
        SlopeCheckVertical(checkPos.position);
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

        if (hit || isGrounded)
        {
            if (isGrounded || hit.distance <= 2f)

            if (isGrounded && !hit)
                return;

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
    
    public void ResizePlayer(float fuel_left)
    {
        mainBody.transform.localScale = Vector3.one * fuel_left;
    }

    public bool OverlapsPosition(Vector2 position)
    {
        foreach (CircleCollider2D cldr in cldrs)
        {
            if (Vector2.Distance(cldr.transform.position, position) <= cldr.radius * mainBody.transform.localScale.x * 1.5f)
            {
                return true;
            }
        }
        return false;
    }
}