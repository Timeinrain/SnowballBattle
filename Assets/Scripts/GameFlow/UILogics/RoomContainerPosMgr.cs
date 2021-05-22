using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;
public class RoomContainerPosMgr : MonoBehaviour
{
	public List<Transform> roomTransforms;
	public List<GameObject> currentRooms;
	public GameObject roomItem;

	private void Awake()
	{
		currentRooms = new List<GameObject> { };
	}

	/// <summary>
	/// InsertRoomItems
	/// </summary>
	public void InsertRoomItem(List<RoomConfig> roomInfos, bool reset = false)
	{
		int transformIndex = 0;
		foreach (var roomInfo in roomInfos)
		{
			GameObject roomObject = Instantiate(roomItem, roomTransforms[transformIndex++]);
			roomObject.GetComponent<RoomButtonUI>().SetInfos(roomInfo.roomName, roomInfo.mapName, roomInfo.currentPlayers.ToString(), roomInfo.gameMode);
			currentRooms.Add(roomObject);
		}
	}

	public void ClearCurrentRooms()
	{
		foreach(var room in currentRooms)
		{
			Destroy(room);
		}
	}

}
