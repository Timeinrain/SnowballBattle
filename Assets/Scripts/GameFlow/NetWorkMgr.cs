using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using Sirenix.OdinInspector;
using ExitGames.Client.Photon;

/// <summary>
/// �����ʼ���ű�
/// ���س����������л�
/// �첽�л�����
/// 
/// =======================READ ME==============================
/// ���ڼ��س�����
/// 1.ʹ�� PanelSwitchFromTo ����
/// �����Ƿ���Ҫ���س�������������������
/// �������Ҫloading���棬��򵥵ĵ���settings ui����
/// ֱ��ʹ�ã�
/// ===============================public void PanelSwitchFromTo(GameObject from, GameObject to, bool needLoading = false)
/// �������ɡ�
/// 2.����Ҫloading���棬��ʹ��������أ���Ҫ�ȴ�ͬ��������Ҫ�ж���ʱ����loading�������Ҫ����Ӧ���Խ���loading��ʱ����һ���˳�loading�������źš�
/// ʹ�÷�����
/// ===============================private void AllowNeedLoadingTransitionEnter() 
/// �������źţ�
/// ��Э�̼������ź�֮����ֹͣloading�����Ĳ��š�
/// ============================================================
/// 2021.05.22��������
/// ����������Ϸ�������߼���
/// 
/// 
/// 
/// 
/// 
/// 
/// Log: 
/// 1.��ͼ�ĸ���ͬ�����пͻ���
/// 2.��ͼ�ĸ���ͬ����������Ϣ
/// 
/// 
/// ����ʱUI������
/// 
/// </summary>
public class NetWorkMgr : MonoBehaviourPunCallbacks
{
	public static NetWorkMgr _Instance;
	#region �������

	[ReadOnly]
	[Header("���ؿͻ��������Ϣ�������޸ģ��Զ�����")]
	public ClientInfo client;

	[Header("������Ϣ")]
	public GameObject roomPrefab;

	public List<int> teamPlayersCount = new List<int> { 0, 0, 0, 0 };
	#endregion
	bool isOnSettle = false;

	#region SYNC
	private Hashtable roomCreationPropertiesCache;
	private string[] roomCreationPropertiesForLobbyCache;
	#endregion

	/// <summary>
	/// For Async load scene.
	/// </summary>
	private void Start()
	{
		_Instance = this;
		PhotonNetwork.AutomaticallySyncScene = true;
		PhotonNetwork.SendRate = 90;
	}

	private void Update()
	{
		if (InOutGameRoomInfo.Instance.isSettlement && !isOnSettle)
		{
			Settle();
		}
	}

	/// <summary>
	/// �������
	/// </summary>
	public void Settle()
	{
		isOnSettle = true;
		UIMgr._Instance.loginUI.SetActive(false);
		UIMgr._Instance.createRoomUI.SetActive(false);
		UIMgr._Instance.settlementUI.SetActive(true);
		UIMgr._Instance.settlementUI.GetComponent<SettlementPanel>().StartSettle(InOutGameRoomInfo.Instance.isVictory);
		//UIMgr._Instance.settlementUI.GetComponent<SettlementPanel>().ShowPlayerScore();
		//UIMgr._Instance.OnGameEndedRejoinRoom();
		//UIMgr._Instance.inRoomUI.GetComponent<InRoom>().RejoinRoomSyncData(InOutGameRoomInfo.Instance.syncData);
	}

	#region Subjective Calls
	/// <summary>
	/// Controlling the Return Button Logic
	/// </summary>
	public void ReturnToLogInPanel()
	{
		UIMgr._Instance.ReturnToLogInPanel();
		PhotonNetwork.Disconnect();
	}

	public void Quit()
	{
		Application.Quit();
	}

	/// <summary>
	/// Using Offline Mode
	/// </summary>
	public void StartOfflineMode()
	{
		//todo: Jump To Offline Panels
	}

	public void SetLobbyRoomMap(Map map)
	{
		var customRoomProperties = new Hashtable()
			{
				{ "Map", map.mapName }
			};
		PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
		string[] roomProperties = new string[] { "Map" };
		PhotonNetwork.CurrentRoom.SetPropertiesListedInLobby(roomProperties);
	}

	public void StartGame()
	{
		if (PhotonNetwork.LocalPlayer.IsMasterClient)
		{
			photonView.RPC("OnLoadingLevel", RpcTarget.All);
			photonView.RPC("SaveData", RpcTarget.All);
			PhotonNetwork.LoadLevel(UIMgr._Instance.inRoomUI.GetComponent<InRoom>().mapInfo.index);
			InOutGameRoomInfo.Instance.WakeMasterMgr();
		}
	}

	public bool IsLocal(string playerID)
	{
		if (PhotonNetwork.LocalPlayer.NickName == playerID)
		{
			return true;
		}
		else return false;
	}

	/// <summary>
	/// Client's Subjective Operation
	/// </summary>
	public void LeaveRoom()
	{
		PhotonNetwork.LeaveRoom();
	}

	/// <summary>
	/// Create and Enter the room
	/// </summary>
	public void CreateRoom(string roomName, byte maxPlayer, GameMode gameMode, Map map)
	{
		RoomOptions roomOp = new RoomOptions()
		{
			MaxPlayers = maxPlayer,
			CustomRoomProperties = new Hashtable()
			{
				{ "GameMode", gameMode.ToString() },
				{ "Map", map.mapName }
			},
			CustomRoomPropertiesForLobby = new string[] { "GameMode", "Map" },
			IsOpen = true,
			IsVisible = true,
			PublishUserId = true,
		};
		roomCreationPropertiesCache = new Hashtable()
			{
				{  "GameMode",gameMode.ToString() },
				{ "Map",map.mapName}
		};
		roomCreationPropertiesForLobbyCache = new string[] { "GameMode", "Map" };
		PhotonNetwork.CreateRoom(roomName, roomOp, default);
	}

	/// <summary>
	/// Join room button clicked
	/// </summary>
	/// <param name="roomID"></param>
	public void JoinRoom(string roomName)
	{
		UIMgr._Instance.JoinRoom();
		PhotonNetwork.JoinRoom(roomName);
	}

	/// <summary>
	/// Join Random Room
	/// </summary>
	public void JoinRandomRoom()
	{
		UIMgr._Instance.JoinRandomRoom();
		PhotonNetwork.JoinRandomRoom();
	}

	public void UpdateTeamInfo(int targetTeamIndex, string playerId)
	{
		photonView.RPC("SwitchTeam", RpcTarget.All, targetTeamIndex, playerId);
	}

	#endregion

	#region overrides
	/// <summary>
	/// Auto call when connected to master
	/// </summary>
	public override void OnConnectedToMaster()
	{
		UIMgr._Instance.AllowNeedLoadingTransitionEnter();
		base.OnConnectedToMaster();
		PhotonNetwork.JoinLobby();
		print(PhotonNetwork.InLobby);
		Debug.Log("Connected!");
	}

	public override void OnLeftRoom()
	{
		base.OnLeftRoom();
		UIMgr._Instance.inRoomUI.GetComponent<InRoom>().ClearRoom();
	}
	public override void OnJoinedLobby()
	{
		base.OnJoinedLobby();
		if (InOutGameRoomInfo.Instance.isSettlement)
		{
			if (InOutGameRoomInfo.Instance.syncData.isThisMaster)
			{
				RoomOptions roomOp = new RoomOptions()
				{
					MaxPlayers = (byte)InOutGameRoomInfo.Instance.syncData.maxPlayers,
					CustomRoomProperties = new Hashtable()
			{
				{ "GameMode", InOutGameRoomInfo.Instance.syncData.gameMode.ToString() },
				{ "Map", GlobalMapInfoMgr.Instance.GetMapByIndex(InOutGameRoomInfo.Instance.syncData.mapIndex).mapName }
			},
					CustomRoomPropertiesForLobby = new string[] { "GameMode", "Map" },
					IsOpen = true,
					IsVisible = true,
					PublishUserId = true,
				};
				PhotonNetwork.CreateRoom(InOutGameRoomInfo.Instance.syncData.roomName, roomOp);
			}
			else
				PhotonNetwork.JoinRoom(InOutGameRoomInfo.Instance.syncData.roomName);
		}
	}

	public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
	{
		base.OnMasterClientSwitched(newMasterClient);
		if (PhotonNetwork.LocalPlayer.IsMasterClient)
		{
			UIMgr._Instance.inRoomUI.GetComponent<InRoom>().MasterClientCheck();
		}
		photonView.RPC("UpdateRoomMaster", RpcTarget.All, newMasterClient.NickName);
	}

	public override void OnJoinRoomFailed(short returnCode, string message)
	{
		base.OnJoinRoomFailed(returnCode, message);
		UIMgr._Instance.OnJoinRoomFailed(message);
	}

	/// <summary>
	/// Why can't join room.
	/// </summary>
	/// <param name="returnCode"></param>
	/// <param name="message"></param>
	public override void OnJoinRandomFailed(short returnCode, string message)
	{
		UIMgr._Instance.OnJoinRandomFailed(message);
		base.OnJoinRandomFailed(returnCode, message);
		Debug.Log(message);
	}

	/// <summary>
	/// What causes the disconnection
	/// </summary>
	/// <param name="cause"></param>
	public override void OnDisconnected(DisconnectCause cause)
	{
		PhotonNetwork.ReconnectAndRejoin();
	}

	/// <summary>
	/// Load the Room Scene
	/// </summary>
	public override void OnJoinedRoom()
	{
		base.OnJoinedRoom();
		UIMgr._Instance.AllowNeedLoadingTransitionEnter();
		UIMgr._Instance.inRoomUI.GetComponent<InRoom>().playerNameUI.text = PhotonNetwork.LocalPlayer.NickName;
		UIMgr._Instance.inRoomUI.GetComponent<InRoom>().InitPlayerContainer();
		
	}

	public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
	{
		base.OnPlayerEnteredRoom(newPlayer);
		UIMgr._Instance.inRoomUI.GetComponent<InRoom>().InsertNewPlayer(newPlayer.NickName, Team.Green);
		if (!PhotonNetwork.LocalPlayer.IsMasterClient) return;
		//�������ͬ����ǰ�ķ��������Ϣ
		var playerInfos = UIMgr._Instance.inRoomUI.GetComponent<InRoom>().playerInfos;
		foreach (var playerInfo in playerInfos)
		{
			if (playerInfo != null)
			{
				photonView.RPC("SyncInRoomPlayerInfo", newPlayer, playerInfo.id, playerInfo.team.GetHashCode(), playerInfo.inRoomPosIndex);
				photonView.RPC("SyncMapInfo", newPlayer, (int)InOutGameRoomInfo.Instance.currentMap.index);
			}
		}
	}

	public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
	{
		base.OnPlayerLeftRoom(otherPlayer);
		if (PhotonNetwork.LocalPlayer.IsMasterClient)
			photonView.RPC("DeleteThePlayer", RpcTarget.All, otherPlayer.NickName);
	}

	public override void OnCreatedRoom()
	{
		base.OnCreatedRoom();
		PhotonNetwork.CurrentRoom.SetCustomProperties(roomCreationPropertiesCache);
		PhotonNetwork.CurrentRoom.SetPropertiesListedInLobby(roomCreationPropertiesForLobbyCache);
		UIMgr._Instance.inRoomUI.GetComponent<InRoom>().InitPlayerContainer();
		if (isOnSettle)
		{
			UIMgr._Instance.inRoomUI.GetComponent<InRoom>().InsertNewPlayer(PhotonNetwork.LocalPlayer.NickName, InOutGameRoomInfo.Instance.syncData.playerTeam, 0);
		}
		else
		{
			UIMgr._Instance.inRoomUI.GetComponent<InRoom>().InsertNewPlayer(PhotonNetwork.LocalPlayer.NickName);
		}
		UIMgr._Instance.inRoomUI.GetComponent<InRoom>().MasterClientCheck();
	}

	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		base.OnRoomListUpdate(roomList);
		foreach (var roomInfo in roomList)
		{
			Hashtable cp = roomInfo.CustomProperties;
			RoomConfig temp = new RoomConfig
			{
				roomName = roomInfo.Name,
				currentPlayers = (byte)roomInfo.PlayerCount,
				gameMode = (string)cp["GameMode"],
				mapName = (string)cp["Map"],
				maxPlayers = roomInfo.MaxPlayers
			};
			UIMgr._Instance.roomListUI.GetComponent<RoomSelection>().roomInfos.Add(temp);
		}
	}

	#endregion

	#region Call by Scripts

	/// <summary>
	/// Responce when Log In button is clicked.
	/// </summary>
	public bool LogIn(string account, string passwd)
	{
		if (!ConnectToChina()) return false;
		PhotonNetwork.SendRate = 60;
		PhotonNetwork.LocalPlayer.NickName = account;
		client = new ClientInfo();
		client.SetInfo(account, passwd);
		print("Connect to China region.");
		return true;
	}

	/// <summary>
	/// Presettings
	/// </summary>
	private bool ConnectToChina()
	{
		PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "cn";
		PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer = true;
		PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = "173ad590-47f0-4573-b1cb-3be35654688b";
		PhotonNetwork.PhotonServerSettings.AppSettings.Server = "ns.photonengine.cn";
		if (PhotonNetwork.ConnectUsingSettings())
		{
			return true;
		}
		else return false;
	}


	/// <summary>
	/// Call When Judging is able to start the game.
	/// </summary>
	/// <returns></returns>
	private bool IsBalanced()
	{
		List<int> cntTemp = new List<int> { };
		foreach (var count in teamPlayersCount)
		{
			if (count != 0) cntTemp.Add(count);
		}
		if (cntTemp.Count == 1) return false;
		int balancedJdg = cntTemp[0];
		foreach (var i in cntTemp)
		{
			if (i != balancedJdg) return false;
		}
		return true;
	}

	#endregion

	#region RPCs
	[PunRPC]
	public void DeleteThePlayer(string playerId)
	{
		UIMgr._Instance.inRoomUI.GetComponent<InRoom>().Delete(playerId);
	}

	[PunRPC]
	public void SyncInRoomPlayerInfo(string playerId, int teamNum, int posIndex)
	{
		Dictionary<int, Team> intToTeam = new Dictionary<int, Team> { { 0, Team.Green }, { 1, Team.Red } };
		UIMgr._Instance.inRoomUI.GetComponent<InRoom>().InsertNewPlayer(playerId, intToTeam[teamNum], posIndex);
		UIMgr._Instance.inRoomUI.GetComponent<InRoom>().MasterClientCheck();
		UIMgr._Instance.inRoomUI.GetComponent<InRoom>().OnRoomMasterSwitched(PhotonNetwork.CurrentRoom.Players[PhotonNetwork.CurrentRoom.MasterClientId].NickName);
	}

	[PunRPC]
	public void SyncMapInfo(int index)
	{
		foreach (var map in GlobalMapInfoMgr.Instance.readInMaps)
		{
			if (map.index == index)
			{
				InOutGameRoomInfo.Instance.currentMap = map;
				UIMgr._Instance.inRoomUI.GetComponent<InRoom>().mapInfo = map;
				break;
			}
		}
	}

	[PunRPC]
	public void UpdateRoomMaster(string playerId)
	{
		UIMgr._Instance.inRoomUI.GetComponent<InRoom>().OnRoomMasterSwitched(playerId);
	}

	[PunRPC]
	public void SwitchTeam(int targetTeamIndex, string whichPlayer)
	{
		Dictionary<int, Team> teamMap = new Dictionary<int, Team> { { 0, Team.Green }, { 1, Team.Red } };
		UIMgr._Instance.inRoomUI.GetComponent<InRoom>().SwitchTeam(teamMap[targetTeamIndex], whichPlayer);
	}

	[PunRPC]
	public void OnLoadingLevel()
	{
		UIMgr._Instance.inRoomUI.GetComponent<InRoom>().SyncInOutGameRoomInfo();
		UIMgr._Instance.loadingPanel.GetComponent<LoadingPanel>().StartLoading();
	}

	[PunRPC]
	public void SaveData()
	{
		InOutGameRoomInfo.Instance.SaveData();
	}

	#endregion

}

public enum TransitionType
{
	OnEnter = 0,
	OnPlay = 1,
	OnExit = 2
}