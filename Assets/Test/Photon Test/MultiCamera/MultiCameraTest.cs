using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
namespace PhotonTest.MultiCameraTest
{
	public class MultiCameraTest : MonoBehaviourPunCallbacks
	{
		// Start is called before the first frame update
		public GameObject sync_Light;
		void Start()
		{
			ConnectToChina();
			PhotonNetwork.SendRate = 90;
		}

		void ConnectToChina()
		{
			PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "cn";
			PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer = true;
			PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = "173ad590-47f0-4573-b1cb-3be35654688b";
			PhotonNetwork.PhotonServerSettings.AppSettings.Server = "ns.photonengine.cn";
			PhotonNetwork.ConnectUsingSettings();
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


			//todo specified the team.
			//===============Need to be=============
			Team teamName = Team.Blue;
			//===============Specified==============

			GameObject go = PhotonNetwork.Instantiate("Player", new Vector3(Random.Range(-1, 1), 3, Random.Range(-1, 1)), Quaternion.identity, 0);
			go.tag = teamName.ToString() + "Team";
			//Join team.
			GameObject teamObj = GameObject.FindWithTag(teamName.ToString() + "Team");
			go.transform.SetParent(teamObj.transform);
			//todo : Sync light data and so on.
			if (PhotonNetwork.IsMasterClient)
			{

			}
			else
			{
				SyncAllDatas();
			}
		}

		public void SyncAllDatas()
		{
			Debug.Log("Sync!");
		}

	}
}