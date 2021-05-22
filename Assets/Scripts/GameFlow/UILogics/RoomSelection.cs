using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Photon.Pun;
using Photon.Realtime;

public class RoomSelection : PanelBase
{
	public GameObject settingsPanel;
	public GameObject createRoomPanel;
	public RoomContainerPosMgr roomPosMgr;
	public Transform roomContainer;
	[ShowInInspector]
	public List<RoomConfig> roomInfos = new List<RoomConfig> { };
	public int roomSelectionPageNum = 0;

	/// <summary>
	/// Register this panel's infos
	/// </summary>
	private void Start()
	{
		currentPanel = gameObject;
		panelEvents += HandleUserInfo;
		UpdateRoomList();
	}

	/// <summary>
	/// Concrete actions for rollback events.
	/// </summary>
	private void HandleUserInfo()
	{
		NetWorkMgr._Instance.client = null;
		GC.Collect();
	}

	/// <summary>
	/// Roll back and panel activity reset.
	/// </summary>
	public override void Return()
	{
		base.Return();
	}

	/// <summary>
	/// Call the Settings Panel
	/// </summary>
	public override void CallSettings()
	{
		base.CallSettings();
		settingsPanel.SetActive(true);
		settingsPanel.GetComponent<SettingsPanel>().SetLastPanel(gameObject);
	}


	/// <summary>
	/// Call CreatingRoom Panel
	/// </summary>
	public void CreateRoom()
	{
		UIMgr._Instance.PanelSwitchFromTo(currentPanel, createRoomPanel);
	}

	/// <summary>
	/// Update roomlist.
	/// </summary>
	/// <param name="rooms"></param>
	public void UpdateRoomList()
	{
		roomPosMgr.ClearCurrentRooms();
		if (roomInfos.Count == 0) return;
		int num = roomInfos.Count;
		List<RoomConfig> insertingPartion = new List<RoomConfig> { };
		for (int i = roomSelectionPageNum * 4; i < Mathf.Min(4 + roomSelectionPageNum * 4, num); i++)
		{
			insertingPartion.Add(roomInfos[i]);
		}
		roomPosMgr.InsertRoomItem(insertingPartion);
		//reset page to 0
		if (roomSelectionPageNum * 4 + 4 < num)
			roomSelectionPageNum++;
		else
		{
			roomSelectionPageNum = 0;
		}
	}

	/// <summary>
	/// Request for RoomList
	/// </summary>
	public void RequestRoomList()
	{
		UpdateRoomList();
	}

	/// <summary>
	/// Start Searching Random Room
	/// </summary>
	public void JoinRandomRoom()
	{
		UIMgr._Instance.JoinRandomRoom();
		NetWorkMgr._Instance.JoinRandomRoom();
	}

}
