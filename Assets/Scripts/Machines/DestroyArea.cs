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
            other.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            PhotonView photonView = PhotonView.Get(other.gameObject);
            photonView.RPC("Die", RpcTarget.All);
        }
        
        if (other.CompareTag("Bomb"))
        {
            Destroy(other.gameObject);
        }
    }
}
