using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class InOutGameRoomInfo : MonoBehaviour
{

	[ShowInInspector]
	public List<Player> inRoomPlayerInfos;
	[ShowInInspector]
	public static InOutGameRoomInfo Instance;
	public string localPlayerId;
	public Map currentMap;

	public int prefabIndex = 0;

	[ShowInInspector]
	public InOutGameRoomSyncData syncData;

	public bool isSettlement = false;
	public bool isVictory = false;

	private void Awake()
	{
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}
		DontDestroyOnLoad(gameObject);
		inRoomPlayerInfos = new List<Player>() { };
		Instance = this;
	}

	public void WakeMasterMgr()
	{
		if (PhotonMasterMgr._Instance != null)
			PhotonMasterMgr._Instance.gameObject.SetActive(true);
	}

	private void Update()
	{
		//if (Input.GetKeyDown(KeyCode.K)) ExitGameRound();
	}

	[Button]
	public void ExitGameRound()
	{
		PhotonMasterMgr._Instance.EndGame();
		if (PhotonNetwork.LocalPlayer.IsMasterClient)
		{
			PhotonMasterMgr._Instance.photonView.RPC("StartSettlement", RpcTarget.All);
		}
		isSettlement = true;
	}

	public void SetInRoomPlayerInfos(List<PlayerInfoInRoom> src)
	{
		foreach (var playerInfo in src)
		{
			if (playerInfo != null)
			{
				if (inRoomPlayerInfos == null) inRoomPlayerInfos = new List<Player>() { };
				inRoomPlayerInfos.Add(new Player() { playerId = playerInfo.id, maxLifeCount = 3, team = playerInfo.team, status = Player.Status.InGame });
				if (NetWorkMgr._Instance.IsLocal(playerInfo.id))
				{
					localPlayerId = playerInfo.id;
				}
			}
		}
	}

	public Player GetPlayerByName(string playerName)
	{
		foreach (var playerInfo in inRoomPlayerInfos)
		{
			if (playerInfo.playerId == playerName)
			{
				return playerInfo;
			}
		}
		return null;
	}

	public void SaveData()
	{
		syncData = UIMgr._Instance.inRoomUI.GetComponent<InRoom>().SaveData();
	}

}
