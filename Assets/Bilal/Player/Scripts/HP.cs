using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains the definition for an object with health.
/// </summary>
public class HP : MonoBehaviour
{
    #region Variables

    // Data
    public float health;
    public float maxHealth = 100.0f;

    // Display
    private HUDController playerHealthHUD;
    public bool treatAsPlayer = false;

    #endregion

    #region Methods

    private void Start()
    {
        health = maxHealth;

        if (treatAsPlayer)
        {
            playerHealthHUD = FindObjectOfType<HUDController>();
            playerHealthHUD.minHealth = 0;
            playerHealthHUD.maxHealth = (int)maxHealth;
            playerHealthHUD.currentHealth = (int)health;
        }
    }

    private void Update()
    {
        if (treatAsPlayer)
        {
            playerHealthHUD.currentHealth = (int)health;
        }
    }

    /// <summary>
    /// Deals the given amount of damage to the object.
    /// </summary>
    /// <param name="amount"></param>
    public void DealDamage(float amount)
    {
        Debug.Log("Player: dealing " + amount + " damage");
        health -= amount;
        Debug.Log("Player: has " + health + " health");

        if (health <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Kills the attached gameobject. If the gameobject is a player, respawn. Otherwise destory the gameObject.
    /// </summary>
    private void Die()
    {
        if (CompareTag("Player"))
        {
            health = maxHealth;
            PlayerMove.respawn();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion
}
