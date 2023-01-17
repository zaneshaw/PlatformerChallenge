using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour {
    private Collider2D col;
    private Rigidbody2D rb;
    public PhysicsMaterial2D noFrictionMaterial;
    public InputMain controls;

    public float maxSpeed;
    public float speed; // 4f
    public float jumpForce; // 6f
    public float groundThreshold; // 0.1f
    public float jumpCooldown; // 0.2f

    private float dir;
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
            if (isGrounded()) {
                col.sharedMaterial = null;

                // Reset jumping state when grounded
                if (jumping) jumping = false;
            } else {
                col.sharedMaterial = noFrictionMaterial;
            }

            // Jump if triggered and not already jumping
            if (controls.Player.Jump.triggered && !jumping) Jump();
        }

        dir = controls.Player.Move.ReadValue<float>();
        Debug.Log(dir);
    }

    private void FixedUpdate() {
        Move();
    }

    private void Move() {
        // Will cause stutter (fix laster :D)
        if (rb.velocity.x < maxSpeed && dir > 0) {
            rb.velocity += new Vector2(dir * speed, 0f);
        }
        if (rb.velocity.x > -maxSpeed && dir < 0) {
            rb.velocity += new Vector2(dir * speed, 0f);
        }
    }

    private void Jump() {
        rb.velocity += new Vector2(0f, jumpForce);

        jumping = true;
        jumpTime = jumpCooldown;
    }

    public bool isGrounded() {
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
