using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MultiCameraTest : MonoBehaviourPunCallbacks
{
	// Start is called before the first frame update
	void Start()
	{
		PhotonNetwork.ConnectUsingSettings();
		PhotonNetwork.SendRate = 90;
	}

	public override void OnConnected()
	{
		base.OnConnected();
	}

	public override void OnConnectedToMaster()
	{
		base.OnConnectedToMaster();
		Debug.Log("Connected!");
		PhotonNetwork.JoinLobby();
	}

	public override void OnJoinedLobby()
	{
		base.OnJoinedLobby();
		Debug.Log("Joined Lobby!");
		PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions() { MaxPlayers = 4 }, default);
	}

	public override void OnJoinedRoom()
	{
		base.OnJoinedRoom();
		Debug.Log("Joined Room!");
		PhotonNetwork.Instantiate("Player", new Vector3(Random.Range(-1, 1), 3, Random.Range(-1, 1)), Quaternion.identity, 0);
	}

}
