using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �ڻ��������Ľ�ɫ�Ļ�¯
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
