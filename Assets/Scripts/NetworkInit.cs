using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkInit : MonoBehaviourPunCallbacks
{
	void Start()
	{
		PhotonNetwork.ConnectUsingSettings();
	}

	public override void OnConnectedToMaster()
	{
		base.OnConnectedToMaster();
		PhotonNetwork.JoinOrCreateRoom("Room", new Photon.Realtime.RoomOptions() { MaxPlayers = 4 }, default);
		Debug.Log(1);
	}

	public override void OnJoinedRoom()
	{
		base.OnJoinedRoom();
		PhotonNetwork.Instantiate("PlayerGroup", new Vector3(Random.Range(-25, 25), 10.0f, Random.Range(-25, 25)), Quaternion.identity, 0);
	}

}
