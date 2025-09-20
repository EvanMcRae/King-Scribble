using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

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
    private bool releasedJumpSinceJump = false;
    public bool needToCutJump = false;
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
    public CinemachineCamera virtualCamera;
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
    [SerializeField] private Animator popAnim;
    private float timeSinceSprint;
    public bool deadLanded = false;
    private float oldFuelLeft = 1.0f;

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
                DOTween.To(() => virtualCamera.Lens.OrthographicSize, x => virtualCamera.Lens.OrthographicSize = x, levelZoom, 1f);
                isSprintMoving = false;
            }
            isSprinting = false;
            sprintSpeedMultiplier = 1f;
        }

        anim.SetBool("isDead", PlayerVars.instance.isDead);
        anim.SetBool("isJumping", isJumping);
        anim.SetBool("isFalling", isFalling && !softFall);
        anim.SetBool("isSprinting", isSprinting || timeSinceSprint < 0.1f);
        anim.SetBool("isGrounded", isGrounded);

        if (Utils.CHEATS_ENABLED && Input.GetKeyDown(KeyCode.Backslash))
        {
            ToggleCheatMode();
        }
        if (PlayerVars.instance.cheatMode)
        {
            CheatModeUpdate();
        }

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
            timeSinceSprint = 0;
            sprintSpeedMultiplier = maxSprintSpeedMultiplier;
            if (moveX != 0 && Mathf.Abs(realVelocity) >= 0.01f && !isSprintMoving && !ChangeScene.changingScene && virtualCamera != null)
            {
                DOTween.To(() => virtualCamera.Lens.OrthographicSize, x => virtualCamera.Lens.OrthographicSize = x, levelZoom + 0.5f, 1f);
                isSprintMoving = true;
            }
            else if ((moveX == 0 || Mathf.Abs(realVelocity) < 0.01f || ChangeScene.changingScene) && isSprintMoving && virtualCamera != null)
            {
                DOTween.To(() => virtualCamera.Lens.OrthographicSize, x => virtualCamera.Lens.OrthographicSize = x, levelZoom, 1f);
                isSprintMoving = false;
            }
        }

        if (Input.GetButtonUp("Sprint"))
        {
            if (isSprintMoving)
            {
                DOTween.To(() => virtualCamera.Lens.OrthographicSize, x => virtualCamera.Lens.OrthographicSize = x, levelZoom, 1f);
                isSprintMoving = false;
            }
            isSprinting = false;
            sprintSpeedMultiplier = 1f;
        }

        if (!isSprinting && timeSinceSprint < 1f)
            timeSinceSprint += Time.deltaTime;
    }

    public void KillTweens()
    {
        DOTween.KillAll();
    }

    void ToggleCheatMode()
    {
        PlayerVars.instance.cheatMode = !PlayerVars.instance.cheatMode;
        Debug.Log((PlayerVars.instance.cheatMode ? "ACTIVATED" : "DEACTIVATED") + " CHEAT MODE");
        rb.bodyType = PlayerVars.instance.cheatMode ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
        //bodyCollider.enabled = !PlayerVars.instance.cheatMode;
    }

    void CheatModeUpdate()
    {
        // increase fly speed -- NOTE: this conflicts with tool selection!
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            cheatSpeed += Input.mouseScrollDelta.x;
        else
            cheatSpeed += Input.mouseScrollDelta.y;
        if (cheatSpeed < 0) cheatSpeed = 0;

        // slow time
        if (Input.GetKeyDown(KeyCode.T))
        {
            Time.timeScale *= 0.5f;
        }

        // raise time
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Time.timeScale *= 2f;
        }

        // unlock pencil
        if (Input.GetKeyDown(KeyCode.I) && !PlayerVars.instance.inventory.hasTool(ToolType.Pencil))
        {
            PlayerVars.instance.inventory.addTool(ToolType.Pencil);
            CollectTool();
            DrawManager.instance.TrySwitchTool(ToolType.Pencil);
        }

        // unlock pen
        if (Input.GetKeyDown(KeyCode.O) && !PlayerVars.instance.inventory.hasTool(ToolType.Pen))
        {
            PlayerVars.instance.inventory.addTool(ToolType.Pen);
            CollectTool();
            DrawManager.instance.TrySwitchTool(ToolType.Pen);
        }

        // unlock eraser
        if (Input.GetKeyDown(KeyCode.P) && !PlayerVars.instance.inventory.hasTool(ToolType.Eraser))
        {
            PlayerVars.instance.inventory.addTool(ToolType.Eraser);
            CollectTool();
            DrawManager.instance.TrySwitchTool(ToolType.Eraser);
        }

        // unlock highlighter
        if (Input.GetKeyDown(KeyCode.LeftBracket) && !PlayerVars.instance.inventory.hasTool(ToolType.Highlighter))
        {
            PlayerVars.instance.inventory.addTool(ToolType.Highlighter);
            CollectTool();
            DrawManager.instance.TrySwitchTool(ToolType.Highlighter);
        }
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
        Vector3 targetVelocity = new Vector2(vars.isDead ? 0 : moveX * calculatedSpeed, rb.linearVelocity.y);

        // sloped movement
        SlopeCheck();
        if (isOnSlope && isGrounded && !isJumping && canWalkOnSlope)
        {
            targetVelocity.Set(vars.isDead ? 0 : moveX * calculatedSpeed * -slopeNormalPerp.x, moveX * calculatedSpeed * -slopeNormalPerp.y, 0.0f);
        }
        // apply velocity, dampening between current and target
        if (!PlayerVars.instance.cheatMode)
        {
            if (moveX == 0.0 && rb.linearVelocity.x != 0.0f)
            {
                if (canWalkOnSlope || !isOnSlope)
                {
                    bodyCollider.sharedMaterial = friction;
                }
                rb.linearVelocity = Vector3.SmoothDamp(rb.linearVelocity, targetVelocity, ref velocity, movementSmoothing * 5f);
            }
            else
            {
                if (!frictionOverride) bodyCollider.sharedMaterial = slippery;
                rb.linearVelocity = Vector3.SmoothDamp(rb.linearVelocity, targetVelocity, ref velocity, movementSmoothing);
            }
        }
        else
        {
            targetVelocity.Set(vars.isDead ? 0 : moveX * (calculatedSpeed + cheatSpeed), moveY * (calculatedSpeed + cheatSpeed), 0.0f);
            rb.linearVelocity = Vector3.SmoothDamp(rb.linearVelocity, targetVelocity, ref velocity, movementSmoothing);
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
                    else if ((PlayerVars.instance.isDead && deadLanded) || !PlayerVars.instance.isDead)
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
            if (isJumping && timeSinceJump > 0.1f && (rb.linearVelocity.y <= 0.001f || beenOnLand > 0.1f))
            {
                jumpSpeedMultiplier = 1f;
                isJumping = false;
                jumpTime = 0f;
                releasedJumpSinceJump = false;
                if (timeSinceJumpPressed > 2 * Time.fixedDeltaTime)
                {
                    if (softFall)
                        softFall = false;
                    else if ((PlayerVars.instance.isDead && deadLanded) || !PlayerVars.instance.isDead)
                        soundPlayer.PlaySound("Player.Land");
                }
            }
            else
            {
                softFall = false;
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
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(new Vector2(0f, jumpForce * rb.mass * (holdingJump ? 1 : 0.7f))); // force added during a jump
            timeSinceJump = 0.0f;
            timeSinceJumpPressed = 0.3f;
            if (holdingJump && releasedJumpSinceJump)
            {
                releasedJumpSinceJump = false;
            }
        }

        if (needToCutJump && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y / 2);
            needToCutJump = false;
        }

        JumpCutCheck();
    }

    void JumpCutCheck()
    {
        if (Input.GetButtonUp("Jump"))
        {
            if (holdingJump && isJumping && rb.linearVelocity.y >= 0)
            {
                if (rb.linearVelocity.y > 0)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0.75f * rb.linearVelocity.y);
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
                Debug.DrawLine(point.point, transform.position - (0.125f * transform.localScale.y * Vector3.up), Color.red);
                if (point.point.y < transform.position.y - 0.125f * transform.localScale.y)
                {
                    onground = true;
                }
            }
            isStuck = false;
            if (!onground && !isOnSlope)
            {
                isStuck = true;
                //moveX *= 0.5f;
            }
        }
        else if (rb.linearVelocity.y < -0.01f)
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
    
    public void ResizePlayer(float fuel_left, bool forceGrow = false) // sorry tronster :(
    {
        int newSize = (int)Mathf.Ceil(fuel_left * SIZE_STAGES);
        bool sizeUpAllowed = false;
        bool growing = false;

        if (newSize < currentSize)
        {
            Debug.Log(newSize + " " + currentSize);
            if (popAnim != null)
            {
                popAnim.gameObject.SetActive(true);
                popAnim.Play("PlayerPop", 0, 0);
            }
            if (newSize != 0)
                soundPlayer.PlaySound("Player.SizeDown");
        }
        else if (newSize > currentSize && !vars.isResetting)
        {
            growing = true;
            if (newSize > currentSize)
            {
                sizeUpAllowed = CanIncreaseSize(currentSize, newSize);
                if (sizeUpAllowed && !forceGrow)
                    soundPlayer.PlaySound("Player.SizeUp");
            }
        }

        fuel_left = Mathf.Ceil(fuel_left * SIZE_STAGES) / SIZE_STAGES;

        if (oldPlayer)
        {
            mainBody.transform.localScale = Vector3.one * fuel_left;
        }
        else
        {
            // Always change size if player is shrinking, only change size if the new collider allows for it
            if ((growing && (sizeUpAllowed || forceGrow)) || (!growing))
            {
                if (newSize != currentSize)
                {
                    if (!growing)
                    {
                        Debug.Log("Shrinking...\ngrowing = " + growing + ", sizeUpAllowed = " + sizeUpAllowed);
                    }
                    else
                    {
                        Debug.Log("Growing...\ngrowing = " + growing + ", sizeUpAllowed = " + sizeUpAllowed);
                    }

                    anim.SetFloat("size", fuel_left);
                    bodyCollider.points = sizeColliders[(int)(fuel_left * SIZE_STAGES)].points;
                    // Debug.Log("Body size is now using " + sizeColliders[(int)(fuel_left * SIZE_STAGES)].name);
                    // Written this way to allow the old player prefab to work
                    groundCheck.gameObject.GetComponent<BoxCollider2D>().size = groundCheckers[(int)(fuel_left * SIZE_STAGES)].GetComponent<BoxCollider2D>().size;
                    groundCheck.offset = groundCheckers[(int)(fuel_left * SIZE_STAGES)].offset;
                }

                currentSize = newSize;
                oldFuelLeft = fuel_left;
            }
            else if (growing && !sizeUpAllowed)
            {
                Hurt();
                Pencil pencil = (Pencil)DrawManager.GetTool(ToolType.Pencil);
                pencil.SetFuel((int)(oldFuelLeft * pencil.GetMaxFuel()));
            }
        }
    }

    // TODO: This detects ground when King Scribble is on the ground (no way) but what this means is that you can only grow in size if you're mid-air
    //       so if the way it's implemented right now isn't a problem you can leave it as is, and otherwise, this is something we gotta work around
    private bool CanIncreaseSize(int currentSize, int newSize)
    {
        // Defensive checks, make sure this function is being used correctly
        if (newSize < currentSize)
        {
            // You're shrinking
            Debug.LogWarning("Calling CanIncreaseSize() with arguments that imply a size decrease, which will always return true");
            return true;
        }

        if (newSize == currentSize)
        {
            // You're the same size
            Debug.LogWarning("Calling CanIncreaseSize() with arguments that imply no change in size, which will always return true");
            return true;
        }

        // Set up trigger to check if the player can resize
        // This trigger will be the same size as what the player would grow into
        BoxCollider2D groundChecker = gameObject.AddComponent<BoxCollider2D>();
        groundChecker.isTrigger = true;
        groundChecker.offset = ((BoxCollider2D)groundCheckers[newSize]).offset;
        groundChecker.size = ((BoxCollider2D)groundCheckers[newSize]).size;

        PolygonCollider2D sizeChecker = gameObject.AddComponent<PolygonCollider2D>();
        sizeChecker.isTrigger = true;
        sizeChecker.points = sizeColliders[newSize].points;

        // Set up contact filter
        ContactFilter2D filter = new();
        filter.SetLayerMask(whatIsGround);
        filter.useTriggers = false;

        // Get a sample of what is being overlapped
        Collider2D[] results = new Collider2D[10];
        int overlapCount = sizeChecker.Overlap(filter, results);

        // If touching anything that is ground, don't grow
        if (overlapCount > 0)
        {
            Collider2D[] groundResults = new Collider2D[10];
            int groundCount = groundChecker.Overlap(filter, groundResults);
            
            if (groundCount > 0)
            {
                foreach (Collider2D sizeCollider in results)
                {
                    if (sizeCollider != null && !groundResults.Contains(sizeCollider))
                    {
                        Destroy(sizeChecker);
                        Destroy(groundChecker);
                        return false;
                    }
                }
            }
        }

        // Otherwise, the player is free to grow
        Destroy(sizeChecker);
        Destroy(groundChecker);
        return true;
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
        soundPlayer.PlaySound("Drawing.PenComplete");
    }

    public void Hurt()
    {
        if (!PlayerVars.instance.isDead)
        {
            anim.SetTrigger("hurt");
            soundPlayer.PlaySound("Player.Hurt");
        }
    }

    public void DeadLanded()
    {
        deadLanded = true;
        if (isGrounded)
            soundPlayer.PlaySound("Player.Land");
    }
}