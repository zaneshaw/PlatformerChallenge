using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerWallJump : MonoBehaviour {
    private Rigidbody2D rb;
    private PlayerController playerController;
    public InputMain controls;

    public float delay; // 0.2f
    public Vector2 force;

    private float airTime;
    private int jumpDir;
    private Collision2D otherCollision;

    private void Awake() {
        controls = new InputMain();
    }

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
    }

    private void Update() {
        CheckWall();

        if (!playerController.IsGrounded()) {
            airTime += Time.deltaTime;
        } else {
            airTime = 0f;
        }
        airTime = Mathf.Clamp(airTime, 0f, delay);

        if (controls.Player.Jump.triggered && jumpDir != 0) {
            // rb.AddForce(new Vector2(force.x * jumpDir, force.y), ForceMode2D.Impulse);
            rb.velocity = new Vector2(rb.velocity.x + force.x * jumpDir, force.y);
        }
    }

    private void CheckWall() {
        jumpDir = 0;

        // If you can try wall jumping
        if (airTime == delay && otherCollision != null) {
            foreach (ContactPoint2D contact in otherCollision.contacts) {
                // If touching a wall
                if (Mathf.Abs(contact.normal.x) == 1) {
                    Debug.DrawRay(contact.point, contact.normal, Color.red);

                    jumpDir = (int)contact.normal.x;
                }
            }
        }
    }

    private void OnCollisionStay2D(Collision2D other) {
        otherCollision = other;
    }

    private void OnCollisionExit2D(Collision2D other) {
        otherCollision = null;
    }

    private void OnEnable() {
        controls.Enable();
    }

    private void OnDisable() {
        controls.Disable();
    }
}
