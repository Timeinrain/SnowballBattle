using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class HealthBar : MonoBehaviour
{
    public Character character;

    private Slider healthBar;

    private void Start()
    {
        healthBar = GetComponent<Slider>();
    }

    void Update()
    {
        healthBar.value = (float)character.Health / character.maxHealth;
    }
}
