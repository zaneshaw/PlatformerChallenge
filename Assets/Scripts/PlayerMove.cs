using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour {
    private Collider2D col;
    private Rigidbody2D rb;
    public InputMain controls;

    public float speed; // 4f
    public float jumpForce; // 6f
    public float groundThreshold; // 0.1f
    public float jumpCooldown; // 0.2f

    private bool jumping;
    private float jumpTime;

    private void Awake() {
        controls = new InputMain();
    }

    private void Start() {
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        // Tick jump cooldown
        if (jumpTime > 0f) jumpTime -= Time.deltaTime;
        jumpTime = Mathf.Clamp(jumpTime, 0f, jumpCooldown);

        // If out of jump cooldown
        if (jumpTime == 0f) {
            // Reset jumping state when grounded
            if (IsGrounded() && jumping) jumping = false;

            // Jump if triggered and not already jumping
            if (controls.Player.Jump.triggered && !jumping) Jump();
        }

        Move();
    }

    private void Move() {
        float dir = controls.Player.Move.ReadValue<float>();

        rb.velocity = new Vector2(dir * speed, rb.velocity.y);
    }
    
    private void Jump() {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);

        jumping = true;
        jumpTime = jumpCooldown;
    }

    private bool IsGrounded() {
        RaycastHit2D hit = Physics2D.BoxCast(col.bounds.center, col.bounds.size, 0f, Vector2.down, groundThreshold, LayerMask.GetMask("Ground"));

        return hit.collider != null;
    }

    private void OnEnable() {
        controls.Enable();
    }
    
    private void OnDisable() {
        controls.Disable();
    }
}
