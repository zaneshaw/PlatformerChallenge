using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    private InputMain controls;
    private Collider2D col;
    private Rigidbody2D rb;

    public enum MovementState {
        Grounded,
        Jumping,
        Midair
    };
    public MovementState movementState { get; private set; }
    private float moveDirection;
    private float moveDeadzone;
    private RaycastHit2D[] hits = new RaycastHit2D[16];
    private float jumpCooldownTimer;
    private float wallJumpDelayTimer;
    private float wallJumpFreezeTimer;

    [Header("Movement")]
    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float maxGroundAngle;
    [SerializeField] private float groundCheckDistance;

    [Header("Jumping")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float wallJumpForce;
    [SerializeField] private float wallJumpDelay;
    [SerializeField] private float wallJumpFreeze;
    [SerializeField] private float maxWallAngle;
    [SerializeField] private float wallCheckDistance;

    private void Awake() {
        controls = new InputMain();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        // Tick jump cooldown
        if (jumpCooldownTimer > 0f) jumpCooldownTimer -= Time.deltaTime;
        jumpCooldownTimer = Mathf.Clamp(jumpCooldownTimer, 0f, jumpCooldown);

        // Tick walljump delay
        if (wallJumpDelayTimer > 0f) wallJumpDelayTimer -= Time.deltaTime;
        wallJumpDelayTimer = Mathf.Clamp(wallJumpDelayTimer, 0f, wallJumpDelay);

        // Tick walljump movement freeze
        if (wallJumpFreezeTimer > 0f) wallJumpFreezeTimer -= Time.deltaTime;
        wallJumpFreezeTimer = Mathf.Clamp(wallJumpFreezeTimer, 0f, wallJumpFreeze);

        // If player is grounded
        if (movementState == MovementState.Grounded) {
            // If jump button is pressed and out of jump cooldown
            if (controls.Player.Jump.triggered && jumpCooldownTimer == 0f) {
                Jump();
            }
        } else if (movementState == MovementState.Midair) {
            if (controls.Player.Jump.triggered && wallJumpDelayTimer == 0f) {
                WallJump();
            }
        }

        moveDirection = controls.Player.Move.ReadValue<float>();

        SetPlayerState();
    }

    private void FixedUpdate() {
        if (wallJumpFreezeTimer == 0f) Move();
    }

    private void Move() {
        if (Mathf.Abs(moveDirection) >= moveDeadzone) {
            rb.velocity += new Vector2(moveDirection * acceleration, 0f);

            if (Mathf.Abs(rb.velocity.x) > maxSpeed) {
                rb.velocity = new Vector2(maxSpeed * Mathf.Sign(rb.velocity.x), rb.velocity.y);
            }
        }

        if (moveDirection == 0f) {
            rb.velocity = new Vector2(rb.velocity.x * (1f - deceleration), rb.velocity.y);
        }
    }

    private void Jump() {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);

        jumpCooldownTimer = jumpCooldown;
    }

    private void WallJump() {
        int direction = 0;
        RaycastHit2D rightWall = Physics2D.BoxCast(col.bounds.center, col.bounds.size, 0f, Vector2.right, wallCheckDistance, LayerMask.GetMask("World"));
        RaycastHit2D leftWall = Physics2D.BoxCast(col.bounds.center, col.bounds.size, 0f, Vector2.left, wallCheckDistance, LayerMask.GetMask("World"));

        if (rightWall.normal.x <= -maxWallAngle) {
            direction = -1;
        } else if (leftWall.normal.x >= maxWallAngle) {
            direction = 1;
        }

        if (direction != 0) {
            wallJumpFreezeTimer = wallJumpFreeze;

            rb.velocity = new Vector2(wallJumpForce * (float)direction, jumpForce);
        }
    }

    public void SetPlayerState() {
        if (IsGrounded()) {
            movementState = MovementState.Grounded;
            wallJumpDelayTimer = wallJumpDelay;
        } else {
            movementState = MovementState.Midair;
        }
    }

    public bool IsGrounded() {
        int hitCount = rb.Cast(Vector2.down, hits, groundCheckDistance);

        for (int i = 0; i < hitCount; i++) {
            if (hits[i].normal.y >= maxGroundAngle) {
                return true;
            }
        }

        return false;
    }

    private void OnEnable() {
        controls.Enable();
    }

    private void OnDisable() {
        controls.Disable();
    }
}
