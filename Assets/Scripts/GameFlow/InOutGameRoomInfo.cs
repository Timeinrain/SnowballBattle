using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class InOutGameRoomInfo : MonoBehaviour
{

	[ShowInInspector]
	List<Player> inRoomPlayerInfos;

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
		inRoomPlayerInfos = new List<Player>() { };
	}

	public void SetInRoomPlayerInfos(List<PlayerInfoInRoom> src)
	{
		foreach (var playerInfo in src)
		{
			if (playerInfo != null)
			{
				inRoomPlayerInfos.Add(new Player() { playerId = playerInfo.id, maxLifeCount = 3, team = playerInfo.team, status = Player.Status.InGame });
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
