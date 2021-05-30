using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Photon.Pun;

public class InOutGameRoomInfo : MonoBehaviour
{

	[ShowInInspector]
	public List<Player> inRoomPlayerInfos;
	public static InOutGameRoomInfo Instance;
	public string localPlayerId;
	public Map currentMap;

	public bool isSettlement = false;

	public float currTime = 0;

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
		inRoomPlayerInfos = new List<Player>() { };
		Instance = this;
	}

	public void StartGameTiming()
	{
		StartCoroutine(Timer());
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

	public void ExitGameRound()
	{
		PhotonNetwork.LoadLevel(0);

	}

	public void SetInRoomPlayerInfos(List<PlayerInfoInRoom> src)
	{
		foreach (var playerInfo in src)
		{
			if (playerInfo != null)
			{
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

}
