using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField]
    public float maxHealth;
    public float health;

    private void Awake()
    {
        health = maxHealth;
    }

    public void TakeDamage(float damageValue)
    {
        health -= damageValue;
    }
}
