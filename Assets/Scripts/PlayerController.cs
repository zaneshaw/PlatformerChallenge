using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    private InputMain controls;
    private Collider2D col;
    private Rigidbody2D rb;

    [Header("Movement")]
    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float maxGroundAngle;
    [SerializeField] private float groundCheckDistance;

    [Header("Jumping")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;

    [SerializeField] private float heightRecord;
    private float moveDirection;
    private float moveDeadzone;
    private RaycastHit2D[] hits = new RaycastHit2D[8];
    private bool jumping;
    private float jumpCooldownTimer;

    private void Awake() {
        controls = new InputMain();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        if (transform.position.y > heightRecord) heightRecord = transform.position.y;

        // Tick jump cooldown
        if (jumpCooldownTimer > 0f) jumpCooldownTimer -= Time.deltaTime;
        jumpCooldownTimer = Mathf.Clamp(jumpCooldownTimer, 0f, jumpCooldown);

        // If out of jump cooldown
        if (jumpCooldownTimer == 0f) {
            if (IsGrounded()) {
                // Reset jumping state when grounded
                if (jumping) jumping = false;
            }

            // Jump if triggered and not already jumping
            if (controls.Player.Jump.triggered && !jumping) Jump();
        }

        moveDirection = controls.Player.Move.ReadValue<float>();
    }

    private void FixedUpdate() {
        if (Mathf.Abs(moveDirection) >= moveDeadzone) {
            Move();
        }

        if (moveDirection == 0) {
            rb.velocity = new Vector2(rb.velocity.x * (1f - deceleration), rb.velocity.y);
        }
    }

    private void Move() {
        rb.velocity += new Vector2(moveDirection * acceleration, 0f);

        if (Mathf.Abs(rb.velocity.x) > maxSpeed) {
            rb.velocity = new Vector2(maxSpeed * Mathf.Sign(rb.velocity.x), rb.velocity.y);
        }
    }

    private void Jump() {
        rb.velocity = new Vector2(0f, jumpForce);

        jumping = true;
        jumpCooldownTimer = jumpCooldown;
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
