using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 融化被冰冻的角色的火炉
/// </summary>
public class Stove : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerController controller = other.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.StartUnfreezeCountdown();
        }

    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController controller = other.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.StopUnfreezeCountdown();
        }

    }
}
