using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour {
    private Collider2D col;
    private Rigidbody2D rb;
    public InputMain controls;

    public float speed = 10f;
    private float gravity = -9.81f;
    private float groundThreshold = 0.1f;

    private void Awake() {
        controls = new InputMain();
    }

    private void Start() {
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        Debug.Log(isGrounded());

        Move();
    }

    private void Move() {
        float dir = controls.Player.Move.ReadValue<float>();

        rb.velocity = new Vector2(dir * speed, rb.velocity.y);
    }

    private bool isGrounded() {
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
