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
    private float moveX, moveY;
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
            int neg = -1;
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
    [SerializeField] private PolygonCollider2D bodyCollider, roofCheck, landCheck, wallCheck;
    [SerializeField] private Collider2D groundCheck;
    [SerializeField] private float groundedRadius, roofedRadius;
    public CinemachineVirtualCamera virtualCamera;
    private float realVelocity;
    private Vector3 lastPosition;
    [SerializeField] private Transform checkPos;
    [SerializeField] private SoundPlayer soundPlayer;
    public bool softFall = true, isStuck = false;
    private bool canJump = true;
    public bool frictionOverride = false;
    private float cheatSpeed = 0.0f;
    public const int SIZE_STAGES = 4;
    public int currentSize = SIZE_STAGES;
    [SerializeField] private List<PolygonCollider2D> sizeColliders = new();
    [SerializeField] private List<Collider2D> groundCheckers = new();
    public bool oldPlayer = true;

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
        facingRight = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (instance == null)
            instance = this;

        if (GameManager.paused)
        {
            // Check for releasing jump during pause
            JumpCutCheck();
            return;
        }
        if (!GameManager.canMove)
        {
            moveX = 0;
            moveY = 0;
            JumpCutCheck();
            if (isSprintMoving)
            {
                DOTween.To(() => virtualCamera.m_Lens.OrthographicSize, x => virtualCamera.m_Lens.OrthographicSize = x, levelZoom, 1f);
                isSprintMoving = false;
            }
            isSprinting = false;
            sprintSpeedMultiplier = 1f;
        }
        
        anim.SetBool("isJumping", isJumping);
        anim.SetBool("isFalling", isFalling);
        anim.SetBool("isSprinting", isSprinting);
        anim.SetBool("isGrounded", isGrounded);

        if (vars.isDead || !GameManager.canMove)
        {
            anim.SetBool("isMoving", false);
            return;
        }

        moveX = Input.GetAxisRaw("Horizontal");
        moveY = Input.GetAxisRaw("Vertical");
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
            if (moveX != 0 && Mathf.Abs(realVelocity) >= 0.01f && !isSprintMoving && !ChangeScene.changingScene && virtualCamera != null)
            {
                DOTween.To(() => virtualCamera.m_Lens.OrthographicSize, x => virtualCamera.m_Lens.OrthographicSize = x, levelZoom + 0.5f, 1f);
                isSprintMoving = true;
            }
            else if ((moveX == 0 || Mathf.Abs(realVelocity) < 0.01f || ChangeScene.changingScene) && isSprintMoving && virtualCamera != null)
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

        // TODO: TEMPORARY CHEAT MODE KEYBIND
        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            PlayerVars.instance.cheatMode = !PlayerVars.instance.cheatMode;
            Debug.Log((PlayerVars.instance.cheatMode ? "ACTIVATED" : "DEACTIVATED") + " CHEAT MODE");
            rb.isKinematic = PlayerVars.instance.cheatMode;
            bodyCollider.enabled = !PlayerVars.instance.cheatMode;
        }
        if (PlayerVars.instance.cheatMode)
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                cheatSpeed += Input.mouseScrollDelta.x;
            else
                cheatSpeed += Input.mouseScrollDelta.y;
            if (cheatSpeed < 0) cheatSpeed = 0;
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
        }

        // calculate speed
        calculatedSpeed = speed * Mathf.Min(jumpSpeedMultiplier * sprintSpeedMultiplier, 2.0f) * Time.fixedDeltaTime;

        // check for ground/roof
        GroundCheck();
        // TODO disabled for now, feels bad
        // RoofCheck();

        // calculate target velocity
        Vector3 targetVelocity = new Vector2(vars.isDead ? 0 : moveX * calculatedSpeed, rb.velocity.y);

        // sloped movement
        SlopeCheck();
        if (isOnSlope && isGrounded && !isJumping && canWalkOnSlope)
        {
            targetVelocity.Set(vars.isDead ? 0 : moveX * calculatedSpeed * -slopeNormalPerp.x, moveX * calculatedSpeed * -slopeNormalPerp.y, 0.0f);
        }

        // apply velocity, dampening between current and target
        if (!PlayerVars.instance.cheatMode)
        {
            if (moveX == 0.0 && rb.velocity.x != 0.0f)
            {
                if (canWalkOnSlope || !isOnSlope)
                {
                    bodyCollider.sharedMaterial = friction;
                }
                rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref velocity, movementSmoothing * 5f);
            }
            else
            {
                if (!frictionOverride) bodyCollider.sharedMaterial = slippery;
                rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref velocity, movementSmoothing);
            }
        }
        else
        {
            targetVelocity.Set(vars.isDead ? 0 : moveX * (calculatedSpeed + cheatSpeed), moveY * (calculatedSpeed + cheatSpeed), 0.0f);
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
                if (fallTime > 0.2f)
                {
                    if (softFall)
                        softFall = false;
                    else
                        soundPlayer.PlaySound("Player.Land");
                }
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
            if (isJumping && timeSinceJump > 0.1f)
            {
                jumpSpeedMultiplier = 1f;
                isJumping = false;
                jumpTime = 0f;
                releasedJumpSinceJump = false;
                if (timeSinceJumpPressed > 2 * Time.fixedDeltaTime)
                {
                    if (softFall)
                        softFall = false;
                    else
                        soundPlayer.PlaySound("Player.Land");
                }
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
        if (Input.GetButtonDown("Jump") && !PauseMenu.unpausedWithSpace)
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
        PauseMenu.unpausedWithSpace = false;

        // incorporates coyote time and input buffering
        float coyoteTimeThreshold = 0.1f;
        bool coyoteTime = lastOnLand < 0.2f && transform.position.y < lastLandHeight - coyoteTimeThreshold;

        if (timeSinceJumpPressed < 0.2f && (isGrounded || coyoteTime) && !isRoofed && !isJumping && canJump)
        {
            anim.SetTrigger("justJumped");
            soundPlayer.PlaySound("Player.Jump");
            // TODO disabled, would reject jumps if on too steep of a slope
            // if (isOnSlope && slopeDownAngle > maxSlopeAngle) return;

            // Add a vertical force to the player
            isGrounded = false;
            isJumping = true;
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(new Vector2(0f, jumpForce * rb.mass * (holdingJump ? 1 : 0.7f))); // force added during a jump
            timeSinceJump = 0.0f;
            timeSinceJumpPressed = 0.3f;
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
            canJump = true;
        }

        // Attempting to fix getting stuck against slopes
        if (bodyCollider.IsTouchingLayers(whatIsGround))
        {
            List<ContactPoint2D> contactPoint2Ds = new();
            bodyCollider.GetContacts(contactPoint2Ds);
            bool onground = false;
            foreach (ContactPoint2D point in contactPoint2Ds)
            {
                Debug.DrawLine(point.point, transform.position - (Vector3.up * 0.125f * transform.localScale.y), Color.red);
                if (point.point.y < transform.position.y - 0.125f * transform.localScale.y)
                {
                    onground = true;
                }
            }
            isStuck = false;
            if (!onground && !isOnSlope)
            {
                isStuck = true;
                moveX *= 0.5f;
            }
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

    public void NotJumping()
    {
        // Get trolled
        isJumping = false;
        anim.SetBool("isJumping", false);
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
        bool slopeHitFront = Physics.Raycast(checkPos, transform.right, slopeCheckDistance, whatIsGround, QueryTriggerInteraction.Ignore);
        bool slopeHitBack = Physics.Raycast(checkPos, -transform.right, slopeCheckDistance, whatIsGround, QueryTriggerInteraction.Ignore);

        RaycastHit2D slopeHitFrontHit = Physics2D.Raycast(checkPos, transform.right, slopeCheckDistance, whatIsGround);
        RaycastHit2D slopeHitBackHit = Physics2D.Raycast(checkPos, -transform.right, slopeCheckDistance, whatIsGround);

        if (slopeHitFront && slopeHitFrontHit)
        {
            isOnSlope = true;
            slopeSideAngle = Vector2.Angle(slopeHitFrontHit.normal, Vector2.up);
        }
        else if (slopeHitBack && slopeHitBackHit)
        {
            isOnSlope = true;
            slopeSideAngle = Vector2.Angle(slopeHitBackHit.normal, Vector2.up);
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
        int newSize = (int)Mathf.Ceil(fuel_left * SIZE_STAGES);
        if (newSize < currentSize && newSize != 0)
        {
            soundPlayer.PlaySound("Player.SizeDown");
        }
        else if (newSize > currentSize)
        {
            soundPlayer.PlaySound("Player.SizeUp");
        }
        currentSize = newSize;

        fuel_left = Mathf.Ceil(fuel_left * SIZE_STAGES) / SIZE_STAGES;
        if (oldPlayer)
            mainBody.transform.localScale = Vector3.one * fuel_left;
        else
        {
            anim.SetFloat("size", fuel_left);
            bodyCollider.points = sizeColliders[(int)(fuel_left * SIZE_STAGES)].points;
            // Written this way to allow the old player prefab to work
            groundCheck.gameObject.GetComponent<BoxCollider2D>().size = groundCheckers[(int)(fuel_left * SIZE_STAGES)].GetComponent<BoxCollider2D>().size;
            groundCheck.offset = groundCheckers[(int)(fuel_left * SIZE_STAGES)].offset;
        }
    }

    public bool OverlapsPosition(Vector2 position)
    {
        return bodyCollider.OverlapPoint(position);
    }

    public void SetFriction(bool active)
    {
        frictionOverride = active;
        if (active)
        {
            bodyCollider.sharedMaterial = friction;
        }
    }

    public void CollectTool()
    {
        soundPlayer.PlaySound("Player.Collect");
    }

    public void CollectSticker()
    {
        // TODO temp
        soundPlayer.PlaySound("Level.Checkpoint");
    }

    public void CollectKey()
    {
        soundPlayer.PlaySound("Level.GetKey");
    }

    public void DeathSound()
    {
        soundPlayer.PlaySound("Player.Die");
    }
}