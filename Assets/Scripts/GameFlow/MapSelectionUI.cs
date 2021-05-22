using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapSelectionUI : PanelBase
{
	public GameObject settingsPanel;
	public CameraMapSelecting cameraFocus;
	public Map currentMap;
	public Text mapName;

	/// <summary>
	/// Camera focus
	/// </summary>
	private void OnEnable()
	{
		cameraFocus.StartSelecting();
	}

	public override void CallSettings()
	{
		base.CallSettings();
		settingsPanel.SetActive(true);
		settingsPanel.GetComponent<SettingsPanel>().SetLastPanel(gameObject);
	}

	public override void Return()
	{
		cameraFocus.SwitchToNormal();
		gameObject.SetActive(false);
		base.Return();
	}

	/// <summary>
	/// Save the result and return to inroom panel.
	/// </summary>
	public void SaveMapSelection()
	{
		lastPanel.GetComponent<InRoom>().SetMapInfo(currentMap);
		Return();
	}

	/// <summary>
	/// When clicked current map was set.
	/// </summary>
	/// <param name="map"></param>
	public void SetCurrentMap(Map map)
	{
		currentMap = map;
		mapName.text = map.mapName;
		mapName.GetComponent<Animator>().SetTrigger("Change");
	}
}
