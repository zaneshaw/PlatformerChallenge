using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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
    private RaycastHit2D[] hits;
    private float jumpCooldownTimer;
    private float wallJumpDelayTimer;
    private float wallJumpFreezeTimer;
    private float invincibilityTimer;

    [Header("Player")]
    [SerializeField] private float invincibilityTime;

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

    [Header("References")]
    [SerializeField] private Tilemap detailMap;

    private void Awake() {
        controls = new InputMain();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        // Tick cooldowns
        TickCooldown(jumpCooldown, ref jumpCooldownTimer);
        TickCooldown(wallJumpDelay, ref wallJumpDelayTimer);
        TickCooldown(wallJumpFreeze, ref wallJumpFreezeTimer);
        TickCooldown(invincibilityTime, ref invincibilityTimer);

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

        if (hits != null && hits.Length > 0) {
            for (int i = 0; i < hits.Length; i++) {
                if (hits[i].collider != null) {
                    Vector3Int pos = detailMap.WorldToCell(hits[i].point);
                    TileBase tile = detailMap.GetTile(pos);
                    if (tile != null && tile.name == "spikes" && invincibilityTimer == 0f) {
                        invincibilityTimer = invincibilityTime;

                        Jump();

                        PlayerLife playerLife = GetComponent<PlayerLife>();
                        float newHealth = playerLife.ChangeHealth(-1f);
                        if (newHealth <= 0f) {
                            playerLife.Die();
                        }
                    }
                }
            }
        }

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

            rb.velocity = new Vector2(wallJumpForce * (float)direction, jumpForce * 0.9f);
        }
    }

    private void SetPlayerState() {
        if (IsGrounded()) {
            movementState = MovementState.Grounded;
            wallJumpDelayTimer = wallJumpDelay;
        } else {
            movementState = MovementState.Midair;
        }
    }

    public void Die() {
        Time.timeScale = 0f;

        Debug.Log("die :(");
    }

    public bool IsGrounded() {
        hits = new RaycastHit2D[16];
        int hitCount = rb.Cast(Vector2.down, hits, groundCheckDistance);

        for (int i = 0; i < hitCount; i++) {
            if (hits[i].normal.y >= maxGroundAngle) {
                return true;
            }
        }

        return false;
    }

    private bool TickCooldown(float cooldown, ref float timer, float? t = null) {
        t ??= Time.deltaTime;

        if (timer > 0f) timer -= (float)t;
        timer = Mathf.Clamp(timer, 0f, cooldown);

        return timer == 0f;
    }

    private void OnEnable() {
        controls.Enable();
    }

    private void OnDisable() {
        controls.Disable();
    }
}
