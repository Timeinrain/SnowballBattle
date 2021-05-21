using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 融化被冰冻的角色的火炉
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class Stove : MonoBehaviour
{
    public float affectRadius;

    private void OnTriggerEnter(Collider other)
    {
        Character character = other.GetComponent<Character>();
        if (character != null)
        {
            character.StartUnfreezeCountdown();
        }

    }

    private void OnTriggerExit(Collider other)
    {
        Character character = other.GetComponent<Character>();
        if (character != null)
        {
            character.StopUnfreezeCountdown();
        }
    }

    private void OnValidate()
    {
        SphereCollider triggerRange = GetComponent<SphereCollider>();
        triggerRange.isTrigger = true;
        triggerRange.radius = affectRadius;
    }
}
