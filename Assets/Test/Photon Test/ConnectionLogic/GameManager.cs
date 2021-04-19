using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using PhotonTest;
/// <summary>
/// Photon for Unity Network 的初始化及控制脚本
/// </summary>
public class GameManager : MonoBehaviourPunCallbacks
{
	public Transform roomContainer;
	public GameObject roomPrefab;

	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		base.OnRoomListUpdate(roomList);
		for (int i = 0; i < roomContainer.childCount; i++)
		{
			if (roomContainer.GetChild(i).gameObject.GetComponentInChildren<Text>().text == roomList[i].Name)
			{
				Destroy(roomContainer.GetChild(i).gameObject);

				if (roomList[i].PlayerCount == 0)
				{
					roomList.Remove(roomList[i]);
				}
			}
		}
		foreach (var room in roomList)
		{
			GameObject newRoom = Instantiate(roomPrefab, roomContainer.position, Quaternion.identity);

			newRoom.GetComponentInChildren<Text>().text = room.Name;

			newRoom.transform.SetParent(roomContainer);

		}
	}
}