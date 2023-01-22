using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLife : MonoBehaviour {
    [SerializeField] public float maxHealth;
    private float health;

    private void Awake() { health = maxHealth; }

    public float GetHealth() { return health; }
    public float SetHealth(float newHealth) { return health = newHealth; }
    public float ChangeHealth(float change) { return health += change; }

    public bool Die() {
        Debug.Log("ded :,(");
        Time.timeScale = 0f;

        return true;
    }
}
