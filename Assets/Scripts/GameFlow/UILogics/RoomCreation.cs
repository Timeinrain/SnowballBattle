using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomCreation : PanelBase
{
	public GameObject settingsPanel;
	public GameObject inRoomUI;
	public InputField roomName;
	public GameMode gameMode;
	public Text maxPlayerCount;
	public Map defaultMap;
	public void SetPVP()
	{
		gameMode = GameMode.PVP;
	}
	public void SetPVE()
	{
		gameMode = GameMode.PVE;
	}
	public void CreateAndJoinRoom()
	{
		UIMgr._Instance.PanelSwitchFromTo(currentPanel, inRoomUI, default, true);
		NetWorkMgr._Instance.CreateRoom(roomName.text, byte.Parse(maxPlayerCount.text), gameMode,defaultMap);
	}
	public override void CallSettings()
	{
		base.CallSettings();
		settingsPanel.SetActive(true);
		settingsPanel.GetComponent<SettingsPanel>().SetLastPanel(gameObject);
	}
	public override void Return()
	{
		base.Return();
	}
}
