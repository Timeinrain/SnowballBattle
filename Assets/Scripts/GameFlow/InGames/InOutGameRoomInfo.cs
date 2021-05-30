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

	[ShowInInspector]
	public InOutGameRoomSyncData syncData;

	public bool isSettlement = false;
	public bool isVictory = false;

	public float currTime = 0;

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

	public void StartGameTiming()
	{
		StartCoroutine(Timer());
	}

	public void WakeMasterMgr()
	{
		if (PhotonMasterMgr._Instance != null)
			PhotonMasterMgr._Instance.gameObject.SetActive(true);
	}

	IEnumerator Timer()
	{
		while (true)
		{
			currTime += Time.deltaTime;
			if (PhotonNetwork.LocalPlayer.IsMasterClient)
			{
				if (currTime >= 180)
				{
					isSettlement = true;
					currTime = 0;
					ExitGameRound();
					break;
				}
			}
			yield return new WaitForEndOfFrame();
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.K)) ExitGameRound();
	}

	[Button]
	public void ExitGameRound()
	{
		PhotonMasterMgr._Instance.EndGame();
		if (PhotonNetwork.LocalPlayer.IsMasterClient)
		{
			PhotonMasterMgr._Instance.photonView.RPC("ReturnToRoom", RpcTarget.AllBuffered);
			inRoomPlayerInfos.Clear();
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
