using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class RoomButtonUI : MonoBehaviour
{
	public Text roomName;
	[OnValueChanged("UpdateMapInfo")]
	public Text mapType;
	[ShowInInspector]
	public Map mapInfo;
	private void OnEnable()
	{
		GetComponent<Animator>().SetTrigger("Refresh");
		//todo:
	}

	private void UpdateMapInfo()
	{
		if (mapType == null) return;
		mapInfo.OnValuaChanged(mapType.text);
	}

	public void JoinRoom()
	{
		NetWorkMgr._Instance.JoinRoom(roomName.text);
		UIMgr._Instance.JoinRoom();
	}
}

