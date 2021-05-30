using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Sirenix.OdinInspector;
using UnityEngine.UI;

/// <summary>
/// 整理一下
/// </summary>
public class InRoom : PanelBase
{
	public GameObject settingsPanel;
	[Header("Sprites")]
	#region Sprites
	[FoldoutGroup("Sprites")]
	[PreviewField] public Sprite green;
	[FoldoutGroup("Sprites")]
	[PreviewField] public Sprite red;
	#endregion
	[Space]
	[Header("In Room Settings")]
	#region InRoomSettings
	public int maxTeamNum = 4;
	public int maxPlayers;
	public Map mapInfo;
	public GameObject mapSelectionButton;
	public GameObject launchButton;
	#endregion
	[Space]
	[Header("All Players' InRoom Infos")]
	#region AllPlayerInfosMgr
	[ReadOnly]
	public List<bool> isEmpty = new List<bool> { };
	[ShowInInspector]
	public Dictionary<int, Sprite> teamIcon;
	public List<PlayerInfoInRoom> playerInfos = new List<PlayerInfoInRoom> { };
	public GameObject playerInfoPrefab;
	public List<RectTransform> playerInfoPos;
	public PlayerInfoInRoom masterPlayer;

	public InOutGameRoomSyncData syncData;

	#endregion
	[Space]
	[Header("Local Client's InRoom Info")]
	#region MyInfo
	[PreviewField]
	public Image teamIndicator;
	public Text playerNameUI;
	[PreviewField]
	public GameObject MochiCharacter;
	public int indexOfCurrentTeam = 0;
	#endregion

	public PlayerInfoInRoom GetPlayerByName(string playerName)
	{
		foreach (var playerInfo in playerInfos)
		{
			if (playerInfo.id == playerName)
			{
				return playerInfo;
			}
		}

		return null;
	}

	/// <summary>
	/// Init the index-team sprite Dictionary
	/// </summary>
	private void Awake()
	{
		teamIcon = new Dictionary<int, Sprite> { { 0, green }, { 1, red } };
		int i = 0;
		while (i++ < maxPlayers)
			isEmpty.Add(true);
	}

	public InOutGameRoomSyncData SaveData()
	{
		InOutGameRoomSyncData data;
		data = new InOutGameRoomSyncData { };
		List<InOutGameRoomSyncData.PlayerInfoForSync> playerList = new List<InOutGameRoomSyncData.PlayerInfoForSync>();
		foreach (var player in playerInfos)
		{
			if (player != null)
			{
				InOutGameRoomSyncData.PlayerInfoForSync thisPlayer = new InOutGameRoomSyncData.PlayerInfoForSync
				{
					id = player.id,
					team = player.team,
					pos = player.inRoomPosIndex,
					isMaster = player == masterPlayer ? true : false
				};
				playerList.Add(thisPlayer);
			}
		}
		data.playerInfoList = (List<InOutGameRoomSyncData.PlayerInfoForSync>)(object)playerList;
		data.mapIndex = mapInfo.index;
		data.playerName = playerNameUI.text;
		data.playerTeam = GetPlayerByName(playerNameUI.text).team;
		data.roomName = PhotonNetwork.CurrentRoom.Name;
		if (PhotonNetwork.LocalPlayer.IsMasterClient)
		{
			data.isThisMaster = true;
		}
		else
		{
			data.isThisMaster = false;
		}
		data.gameMode = GameMode.PVP;
		data.maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;

		//todo : 获取角色服装信息 ---------------------------------------------------------------- 
		return data;
	}

	public void RejoinRoomSyncData(InOutGameRoomSyncData datas)
	{
		MochiCharacter.SetActive(false);
		int cnt = 0;
		while (cnt++ < datas.maxPlayers)
			playerInfos.Add(null);
		teamIndicator.sprite = teamIcon[datas.playerTeam.GetHashCode()];
		playerNameUI.text = datas.playerName;
		mapInfo = GlobalMapInfoMgr.Instance.GetMapByIndex(datas.mapIndex);
	}

	/// <summary>
	/// If this client is the master client in room. Do some relative things.
	/// </summary>
	public void MasterClientCheck()
	{
		if (PhotonNetwork.LocalPlayer.IsMasterClient)
		{
			mapSelectionButton.SetActive(true);
			mapSelectionButton.GetComponent<Animator>().SetTrigger("Enter");
			launchButton.SetActive(true);
			launchButton.GetComponent<Animator>().SetTrigger("Enter");
			string playerId = PhotonNetwork.LocalPlayer.NickName;
			foreach (var playerInfo in playerInfos)
			{
				if (playerId == playerInfo.id)
				{
					playerInfo.AnimateMasterClient();
					masterPlayer = playerInfo;
					break;
				}
			}
		}
		else
		{
			mapSelectionButton.SetActive(false);
			launchButton.SetActive(false);
		}
	}

	/// <summary>
	/// When a new room master success the old one. Animate on all client to update the "STAR" indicator
	/// </summary>
	/// <param name="playerId"></param>
	public void OnRoomMasterSwitched(string playerId)
	{
		foreach (var playerInfo in playerInfos)
		{
			if (playerInfo != null)
			{
				if (playerId == playerInfo.id)
				{
					playerInfo.AnimateMasterClient();
					break;
				}
			}
		}
	}

	/// <summary>
	/// To Synchronize the information between ingame scene and inroom scene.
	/// </summary>
	public void SyncInOutGameRoomInfo()
	{
		InOutGameRoomInfo.Instance.SetInRoomPlayerInfos(playerInfos);
		InOutGameRoomInfo.Instance.currentMap = mapInfo;
	}

	/// <summary>
	/// Call when Left switch Onclick
	/// </summary>
	public void SwitchLeft()
	{
		if (indexOfCurrentTeam == 0)
		{
			indexOfCurrentTeam = maxTeamNum - 1;
		}
		else
		{
			indexOfCurrentTeam--;
		}
		teamIndicator.sprite = teamIcon[indexOfCurrentTeam];
		NetWorkMgr._Instance.UpdateTeamInfo(indexOfCurrentTeam, PhotonNetwork.LocalPlayer.NickName);
	}

	/// <summary>
	/// Call when Right Switch OnClick
	/// </summary>
	public void SwitchRight()
	{
		if (indexOfCurrentTeam == maxTeamNum - 1)
		{
			indexOfCurrentTeam = 0;
		}
		else
		{
			indexOfCurrentTeam++;
		}
		teamIndicator.sprite = teamIcon[indexOfCurrentTeam];
		NetWorkMgr._Instance.UpdateTeamInfo(indexOfCurrentTeam, PhotonNetwork.LocalPlayer.NickName);
	}

	/// <summary>
	/// When exit room. The client need to rejoin the lobby and update the inroom infos for all inroom clients.
	/// And If this is a master Client. Photon Automatically Switch master client to another.
	/// </summary>
	public override void Return()
	{
		NetWorkMgr._Instance.LeaveRoom();
		base.Return();
	}

	/// <summary>
	/// Call Settings Panel
	/// </summary>
	public override void CallSettings()
	{
		base.CallSettings();
		settingsPanel.SetActive(true);
		settingsPanel.GetComponent<SettingsPanel>().SetLastPanel(gameObject);
	}

	/// <summary>
	/// Select Map
	/// </summary>
	public void SelectMap()
	{
		UIMgr._Instance.StartSelectingMap();
	}

	/// <summary>
	/// Set the map for current Room.
	/// </summary>
	/// <param name="_mapInfo"></param>
	public void SetMapInfo(Map _mapInfo)
	{
		mapInfo = _mapInfo;
		InOutGameRoomInfo.Instance.currentMap = _mapInfo;
		NetWorkMgr._Instance.photonView.RPC("SyncMapInfo", RpcTarget.AllBufferedViaServer, (int)mapInfo.index);
		NetWorkMgr._Instance.SetLobbyRoomMap(_mapInfo);
	}

	/// <summary>
	/// Init.  (Neglectable
	/// </summary>
	[Button]
	public void InitPlayerContainer()
	{
		for (int i = 0; i < maxPlayers; i++)
		{
			isEmpty.Add(true);
			playerInfos.Add(null);
		}
	}

	/// <summary>
	/// Find which place to insert new player.
	/// </summary>
	/// <returns></returns>
	private int FindFirstEmptyPos()
	{
		for (int i = 0; i < maxPlayers; i++)
		{
			if (isEmpty[i])
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Insert a new Player with name(replaced with playerID), team and pos Index.
	/// </summary>
	/// <param name="playerId">Nick Name</param>
	/// <param name="playerTeam"> Init Team is blue</param>
	/// <param name="index">Position Index, Automatically specified.</param>
	[Button("Test Insert")]
	public void InsertNewPlayer(string playerId = "test", Team playerTeam = Team.Red, int index = -1)
	{
		int insertIndex = FindFirstEmptyPos();
		if (index != -1)
		{
			insertIndex = index;
		}
		if (insertIndex == -1) return;
		GameObject playerInfoObject =
		Instantiate(playerInfoPrefab, playerInfoPos[insertIndex]);
		playerInfoObject.GetComponent<PlayerInfoInRoom>().id = playerId;
		playerInfoObject.GetComponent<PlayerInfoInRoom>().team = playerTeam;
		playerInfoObject.GetComponent<PlayerInfoInRoom>().inRoomPosIndex = insertIndex;
		playerInfoObject.GetComponent<PlayerInfoInRoom>().UpdateInfo();
		playerInfos[insertIndex] = playerInfoObject.GetComponent<PlayerInfoInRoom>();
		isEmpty[insertIndex] = false;
	}

	/// <summary>
	/// Called When Exit Room
	/// </summary>
	public void ClearRoom()
	{
		foreach (var playerInfo in playerInfos)
		{
			if (playerInfo != null)
				Destroy(playerInfo.gameObject);
		}
		isEmpty.Clear();
		playerInfos.Clear();
	}

	/// <summary>
	/// Call by rpcs. Update specific player's room Info.
	/// </summary>
	/// <param name="targetTeam"></param>
	/// <param name="playerId"></param>
	public void SwitchTeam(Team targetTeam, string playerId)
	{
		PlayerInfoInRoom targetPlayer = null;
		foreach (var playerInfo in playerInfos)
		{
			if (playerInfo != null)
			{
				if (playerInfo.id == playerId)
				{
					targetPlayer = playerInfo;
					break;
				}
			}
		}
		if (targetPlayer == null) return;
		targetPlayer.team = targetTeam;
		targetPlayer.UpdateInfo();
	}

	/// <summary>
	/// Camera's Calling.
	/// </summary>
	/// <param name="active"></param>
	public void MochiInactive(bool active = false)
	{
		MochiCharacter.SetActive(active);
	}

	/// <summary>
	/// When a Player Exit room.
	/// </summary>
	/// <param name="playerId"></param>
	[Button("Test Delete")]
	public void Delete(string playerId)
	{
		for (int i = 0; i < playerInfos.Count; i++)
		{
			if (playerInfos[i] != null)
			{
				if (playerInfos[i].id == playerId && isEmpty[i] == false)
				{
					isEmpty[i] = true;
					playerInfos[i].ExitRoom();
					break;
				}
			}
		}
	}

}


public struct InOutGameRoomSyncData
{
	public struct PlayerInfoForSync
	{
		public string id;
		public Team team;
		public int pos;
		public bool isMaster;
	}

	public List<PlayerInfoForSync> playerInfoList;
	public string playerName;
	public Team playerTeam;
	public int playerClothIndex;
	public int mapIndex;
	public string roomName;
	public bool isThisMaster;
	public int maxPlayers;
	public GameMode gameMode;
}
