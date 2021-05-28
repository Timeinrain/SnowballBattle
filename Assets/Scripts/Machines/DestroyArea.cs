using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DestroyArea : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null && PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            // ʹ��ɫǿ������(ֻ����������)
            PhotonView photonView = PhotonView.Get(other.gameObject);
            Debug.Log(photonView);
            photonView.RPC("Die", RpcTarget.All);
        }
        
        if (other.CompareTag("Bomb"))
        {
            Destroy(other.gameObject);
        }
    }
}
