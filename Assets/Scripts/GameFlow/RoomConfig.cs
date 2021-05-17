using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// Share Information about a room info
/// </summary>
public class RoomConfig : MonoBehaviourPun
{
	GameObject roomMap;
	int roomPlayerCount;
	string MapType;
	string roomName;
	int maxPlayers;
	private void OnEnable()
	{
		int playerCnt = 0;
		int.TryParse(transform.Find("RoomPlayerCountDetail").gameObject.GetComponent<Text>().text, out playerCnt);
		roomPlayerCount = playerCnt;
		roomMap = transform.Find("RoomMap").gameObject;
		MapType = transform.Find("MapType").gameObject.GetComponent<Text>().text;
		roomName = transform.Find("RoomNameDetail").gameObject.GetComponent<Text>().text;
	}

	/// <summary>
	/// Refresh Room Info
	/// </summary>
	public virtual void UpdateRoomInfo()
	{
		int playerCnt = 0;
		int.TryParse(transform.Find("RoomPlayerCountDetail").gameObject.GetComponent<Text>().text, out playerCnt);
		roomPlayerCount = playerCnt;
		roomMap = transform.Find("RoomMap").gameObject;
		MapType = transform.Find("MapType").gameObject.GetComponent<Text>().text;
		roomName = transform.Find("RoomNameDetail").gameObject.GetComponent<Text>().text;
	}

	public virtual void UpdateRoomInfo(RoomInfo room)
	{
		roomPlayerCount = room.PlayerCount;
		//roomMap = room.MapInfo;
		//MapType = transform.Find("MapType").gameObject.GetComponent<Text>().text;
		roomName = room.Name;
		maxPlayers = room.MaxPlayers;
	}

	public void JoinRoom()
	{
		FindObjectOfType<NetWorkMgr>().JoinRoom(roomName);
	}

}
