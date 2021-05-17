using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Photon.Pun;
using Photon.Realtime;

[ExecuteInEditMode]
/// <summary>
/// 
/// </summary>
public class RoomSelection : PanelBase
{
	public GameObject settingsPanel;
	public GameObject createRoomPanel;
	public List<GameObject> roomInfos;

	/// <summary>
	/// Register this panel's infos
	/// </summary>
	private void Start()
	{
		currentPanel = gameObject;
		panelEvents += handleUserInfo;
	}

	/// <summary>
	/// Concrete actions for rollback events.
	/// </summary>
	private void handleUserInfo()
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
		List<RoomInfo> roomInfos = NetWorkMgr._Instance.GetRoomInfos();
		int num = roomInfos.Count;
		for(int i = 0; i < 4; i++)
		{
			
		}
	}

	/// <summary>
	/// Request for RoomList
	/// </summary>
	public void RequestRoomList()
	{
		NetWorkMgr._Instance.GetRoomInfos();


	}

	/// <summary>
	/// Start Searching Random Room
	/// </summary>
	public void JoinRandomRoom()
	{

	}

	/// <summary>
	/// Refresh the existing rooms
	/// </summary>
	public void RefreshRoomList()
	{
		
	}
}
