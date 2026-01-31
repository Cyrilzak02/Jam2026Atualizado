using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    bool movementLocked = false;
    float movementLockTimer = 0f;


    [Header("Jump")]
    public float jumpForce = 0.5f;
    public int maxJumps = 2;

    [Header("Glide")]
    public float glideGravityScale = 0.3f;
    public float maxFallSpeed = -2f;
    public float maxGlideTime = 2f; // máximo de 2 segundos
    float glideTimer = 0f;


    [Header("Dash")]
    public float dashSpeed = 12f;
    public float dashDuration = 0.15f;

    [Header("Wall Slide")]
    public float wallSlideSpeed = -1.5f;
    public Vector2 wallCheckOffset = new Vector2(0.6f, 0f);
    public float wallCheckRadius = 0.15f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayerMask;

    Rigidbody2D rb;
    PlayerInput playerInput;

    InputAction moveAction;
    InputAction jumpAction;

    Vector2 moveInput;
    public bool isGrounded { get; private set; }
    bool isGliding;
    bool isDashing;
    bool isWallSliding;
    bool jumpRequested;

    int jumpsRemaining;
    float dashTimer;
    float normalGravityScale;
    Vector3 originalScale;

    // --- New fields for abilities / external controls ---
    bool externallyControlledGlideActive = false; // set by ability system
    public bool wallGripEnabled = false;            // when wall-jump ability equipped

    float lastMoveDir = 1f; // to pick dash direction when no input
    public event Action OnJumped; // other systems (abilities) can subscribe

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();

        normalGravityScale = rb.gravityScale;
        originalScale = transform.localScale;
        
        jumpsRemaining = 1;

        SetupGroundCheck();
    }

    IEnumerator Start()
    {
        // Espera 1 frame para o PlayerAbilities e PlayerInput inicializarem
        yield return null;

        CheckGrounded();

        var abilities = GetComponent<PlayerAbilities>();
        bool hasDoubleJump = abilities != null && abilities.IsEquipped(AbilityType.DoubleJump);

        // Se estiver no chão, sempre começa com 1 pulo, mesmo sem double jump
        jumpsRemaining = isGrounded ? 1 : 0;

        // Se tiver double jump, garante 1 extra no ar depois do primeiro pulo
        if (hasDoubleJump)
            jumpsRemaining = 1;

        Debug.Log($"[Init] Grounded: {isGrounded}, hasDoubleJump: {hasDoubleJump}, jumpsRemaining: {jumpsRemaining}");
    }


    void OnEnable()
    {
        moveAction = playerInput.actions.FindAction("Move", true);
        jumpAction = playerInput.actions.FindAction("Jump", true);
    }

    void Update()
    {
        // read movement
        if (moveAction != null)
            moveInput = moveAction.ReadValue<Vector2>();
        else
            moveInput = Vector2.zero;

        if (moveInput.x != 0f)
            lastMoveDir = Mathf.Sign(moveInput.x);

        if (jumpAction != null && jumpAction.WasPressedThisFrame() && !isDashing)
        {
            bool canWallJump = (wallGripEnabled || isWallSliding) && IsTouchingWall();
            bool touchingWallWithoutAbility = !wallGripEnabled && IsTouchingWall();

            // impede pulo normal se estiver encostando na parede sem a habilidade
            if (touchingWallWithoutAbility)
            {
                Debug.Log("[JumpBlocked] Tried to jump on wall without wall jump ability.");
                return;
            }

            // permite pular se ainda tiver pulos OU se puder fazer wall jump
            if (jumpsRemaining > 0 || canWallJump)
                jumpRequested = true;
        }

        // GLIDE — segurar botão de pulo (space)
        bool isHoldingJump = jumpAction != null && jumpAction.ReadValue<float>() > 0f;
        bool canGlide = !isGrounded && !IsTouchingWall() && rb.linearVelocity.y <= 0f;

        isGliding = externallyControlledGlideActive || (canGlide && isHoldingJump);
        if (isGliding)
        {
            glideTimer += Time.deltaTime;
            if (glideTimer > maxGlideTime)
                isGliding = false; // desativa ao passar do tempo
        }
        else
        {
            glideTimer = 0f;
        }



        // dash by keyboard (legacy); abilities will call StartDashFromAbility()
        if (!isDashing && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartDash();
        }
    }

    void FixedUpdate()
    {
        
        if (movementLocked)
        {
            movementLockTimer -= Time.fixedDeltaTime;
            if (movementLockTimer <= 0f)
                movementLocked = false;
        }

        CheckGrounded();

        if (isDashing)
        {
            DashMove();
            return;
        }

        // WALL SLIDE — apenas se a habilidade estiver ativa
        if (wallGripEnabled && !isGrounded && IsTouchingWall() && rb.linearVelocity.y <= 0f)
        {
            isWallSliding = true;

            // desce devagar
            if (rb.linearVelocity.y < wallSlideSpeed * 0.5f)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, wallSlideSpeed * 0.5f);
        }
        else
        {
            isWallSliding = false;
            Move();
        }


        if (jumpRequested)
        {
            Jump();
            jumpRequested = false;
        }

        if (isGliding)
            ApplyGlide();
        else
            rb.gravityScale = normalGravityScale;

        // Ajuste de gravidade para curva suave no ar
        if (!isGrounded && rb.linearVelocity.y < 0)
        {
            // aumenta a gravidade progressivamente ao cair
            rb.gravityScale = Mathf.Lerp(rb.gravityScale, normalGravityScale * 1.5f, Time.fixedDeltaTime * 2f);
        }
        else if (isGrounded)
        {
            // reseta ao tocar o chão
            rb.gravityScale = normalGravityScale;
        }

    }

    // ================= MOVEMENT =================

    void Move()
    {
        if (movementLocked)
            return;

        // controle aéreo suave
        float targetVelocityX = moveInput.x * moveSpeed;
        float smoothing = isGrounded ? 1f : 0.1f; // menos controle no ar
        rb.linearVelocity = new Vector2(
            Mathf.Lerp(rb.linearVelocity.x, targetVelocityX, smoothing),
            rb.linearVelocity.y
        );

    }

    public void Jump()
    {
        var abilities = GetComponent<PlayerAbilities>();
        bool hasDoubleJump = abilities != null && abilities.IsEquipped(AbilityType.DoubleJump);

        // WALL JUMP =====================================================
        if ((wallGripEnabled || isWallSliding) && IsTouchingWall())
        {
            bool touchingRight = IsTouchingWallRight();
            bool touchingLeft  = IsTouchingWallLeft();
            int direction = touchingRight ? -1 : (touchingLeft ? 1 : 0);
            if (direction == 0) return;

            float horizontalSpeed = jumpForce * 2.2f;
            float verticalSpeed   = jumpForce * 0.65f;

            rb.linearVelocity = new Vector2(direction * horizontalSpeed, verticalSpeed);

            wallGripEnabled = false;
            isWallSliding = false;
            isGliding = false;
            rb.gravityScale = normalGravityScale * 1.25f;

            movementLocked = true;
            movementLockTimer = 0.15f;

            // 🚫 Bloqueia qualquer pulo extra após wall jump
            jumpsRemaining = 0;

            Debug.Log($"[WallJump] dir={direction}, vel={rb.linearVelocity}");
            OnJumped?.Invoke();
            return;
        }

        // NORMAL JUMP =====================================================
        if (isGrounded)
        {
            // pulo do chão
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

            // Reseta contador — se tiver double jump, ganha 1 extra
            jumpsRemaining = hasDoubleJump ? 1 : 0;

            OnJumped?.Invoke();
            return;
        }

        // DOUBLE JUMP =====================================================
        if (!isGrounded && jumpsRemaining > 0 && hasDoubleJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpsRemaining = 0; // consome o double jump

            Debug.Log("[DoubleJump] Activated");
            OnJumped?.Invoke();
            return;
        }

        // Se não puder pular (sem pulos restantes)
        Debug.Log("[JumpBlocked] No jumps remaining");
    }


    // ================= GLIDE =================

    void ApplyGlide()
    {
        rb.gravityScale = glideGravityScale;

        if (rb.linearVelocity.y < maxFallSpeed)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, maxFallSpeed);
    }

    /// <summary>
    /// Called by abilities to force enable/disable glide (e.g. while holding ability button).
    /// </summary>
    public void SetExternalGlide(bool active)
    {
        externallyControlledGlideActive = active;
        if (!active)
            rb.gravityScale = normalGravityScale;
    }

    // ================= DASH =================

    // original StartDash kept for legacy input
    void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        transform.localScale = new Vector3(originalScale.x, originalScale.y * 0.6f, originalScale.z);
    }

    /// <summary>
    /// Start dash from an external caller (ability). Uses lastMoveDir if no input.
    /// </summary>
    public void StartDashFromAbility(float customSpeed = -1f, float customDuration = -1f)
    {
        if (isDashing) return;

        isDashing = true;
        dashTimer = customDuration > 0f ? customDuration : dashDuration;

        float dir = lastMoveDir != 0f ? Mathf.Sign(lastMoveDir) : 1f;
        float speed = customSpeed > 0f ? customSpeed : dashSpeed;
        rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);

        transform.localScale = new Vector3(originalScale.x, originalScale.y * 0.6f, originalScale.z);
    }

    void DashMove()
    {
        dashTimer -= Time.fixedDeltaTime;
        // while dashing maintain the velocity set when dash started
        // (we already set rb.velocity in StartDashFromAbility)
        if (dashTimer <= 0f)
        {
            isDashing = false;
            transform.localScale = originalScale;
        }
    }

    // ================= WALL SLIDE =================

    void ApplyWallSlide()
    {
        isWallSliding = true;

        // only clamp Y (existing behavior)
        if (rb.linearVelocity.y < wallSlideSpeed)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, wallSlideSpeed);
    }

    bool IsTouchingWall()
    {
        return IsTouchingWallRight() || IsTouchingWallLeft();
    }

    bool IsTouchingWallRight()
    {
        Vector3 right = transform.position + (Vector3)wallCheckOffset;
        return Physics2D.OverlapCircle(right, wallCheckRadius, groundLayerMask);
    }

    bool IsTouchingWallLeft()
    {
        Vector3 left = transform.position - (Vector3)wallCheckOffset;
        return Physics2D.OverlapCircle(left, wallCheckRadius, groundLayerMask);
    }

    // expose wall touch for external checks
    public bool IsTouchingWallPublic() => IsTouchingWall();

    // ================= GROUND =================

    void CheckGrounded()
    {
        bool wasGrounded = isGrounded;

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayerMask
        );

        if (isGrounded)
        {
            var abilities = GetComponent<PlayerAbilities>();
            bool hasDoubleJump = abilities != null && abilities.IsEquipped(AbilityType.DoubleJump);

            // sempre garante 1 pulo quando está no chão
            jumpsRemaining = 1;
            rb.gravityScale = normalGravityScale;
        }


    }

    void SetupGroundCheck()
    {
        if (groundCheck == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.SetParent(transform);
            gc.transform.localPosition = new Vector3(0, -0.8f, 0);
            groundCheck = gc.transform;
        }
    }

    // ================= ABILITY HELPERS =================

    /// <summary>
    /// Force a single extra jump (used by double-jump ability). This ignores jumpsRemaining.
    /// </summary>
    public void ForceDoubleJump()
    {
        if (isGrounded) return; // only in air
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        OnJumped?.Invoke();
    }

    /// <summary>
    /// Enable / disable wall-grip behaviour (used when wall-jump ability is equipped).
    /// </summary>
    public void EnableWallGrip(bool enable)
    {
        wallGripEnabled = enable;
        if (!enable)
            isWallSliding = false;
    }

    IEnumerator TemporarilyDisableMovement(float time)
    {
        float originalSpeed = moveSpeed;
        moveSpeed = 0f;
        yield return new WaitForSeconds(time);
        moveSpeed = originalSpeed;
    }


    #if UNITY_EDITOR
        void OnGUI()
        {
            // Mostra info de debug básica no console, mas sem spammar
            if (Input.GetKeyDown(KeyCode.F3))
            {
                var abilities = GetComponent<PlayerAbilities>();

                Debug.Log(
                    $"[DEBUG] " +
                    $"Grounded: {isGrounded} | " +
                    $"JumpsRemaining: {jumpsRemaining} | " +
                    $"IsGliding: {isGliding} | " +
                    $"IsDashing: {isDashing} | " +
                    $"IsWallSliding: {isWallSliding} | " +
                    $"WallGripEnabled: {wallGripEnabled} | " +
                    $"Equipped: {(abilities != null ? abilities.GetEquippedName() : "None")}"
                );
            }
        }
    #endif
}
