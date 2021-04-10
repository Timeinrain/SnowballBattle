using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;

namespace networkTest
{

	public class NetworkInit : MonoBehaviourPunCallbacks
	{
		public GameObject loginUI;
		public GameObject roomListUI;
		public Text userName;
		public Text roomName;
		public byte maxPlayers = 4;
		private void Start()
		{
			PhotonNetwork.ConnectUsingSettings();
		}

		public override void OnConnectedToMaster()
		{
			base.OnConnectedToMaster();
			Debug.Log("Connected to master.");
			loginUI.SetActive(true);
		}

		public void LoginButtonOnClick()
		{
			PhotonNetwork.NickName = userName.text;
			loginUI.SetActive(false);
			PhotonNetwork.JoinLobby(default);
		}

		public void JoinRoomButtonOnClick(string roomNameTemp = "default")
		{
			if (roomNameTemp == "default")
				PhotonNetwork.JoinOrCreateRoom(roomName.text, new RoomOptions() { MaxPlayers = maxPlayers }, default);
			else
			{
				PhotonNetwork.JoinOrCreateRoom(roomNameTemp, new RoomOptions() { MaxPlayers = maxPlayers }, default);
			}
		}

		public override void OnJoinedRoom()
		{
			base.OnJoinedRoom();
			Debug.Log("Joined room!");
			PhotonNetwork.LoadLevel(1);
		}
		public override void OnJoinedLobby()
		{
			roomListUI.SetActive(true);
			base.OnJoinedLobby();
			Debug.Log("Joined Lobby!");
		}
	}
}