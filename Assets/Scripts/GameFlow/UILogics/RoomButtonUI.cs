using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class RoomButtonUI : MonoBehaviour
{
	public Text roomName;
	public Text mapType;
	public Text currentPlayers;
	public Text gameMode;
	private void OnEnable()
	{
		GetComponent<Animator>().SetTrigger("Refresh");
	}

	public void SetInfos(string _roomName, string _mapName, string _currentPlayers, string _mode)
	{
		roomName.text = _roomName;
		mapType.text = _mapName;
		currentPlayers.text = _currentPlayers;
		gameMode.text = _mode;
	}

	public void JoinRoom()
	{
		UIMgr._Instance.AllowNeedLoadingTransitionEnter(false);
		NetWorkMgr._Instance.JoinRoom(roomName.text);
	}
}

