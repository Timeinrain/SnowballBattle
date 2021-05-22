using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
[CreateAssetMenu(fileName = "Map", menuName = "CreateMap/create")]
public class Map : ScriptableObject
{
	//[PreviewField(100, Alignment = ObjectFieldAlignment.Left)]
	//public Texture mapImage;

	public string mapName;

	public string mapDescription;

	[PreviewField(100, Alignment = ObjectFieldAlignment.Left)]
	public GameObject tinyModel;

	public byte index;

	public MapTheme mapTheme;

	private bool ValidationCheck(string mapName, byte index = 0, MapTheme theme = MapTheme.Arena)
	{
		return !GlobalMapInfoMgr.IsMapExisted(mapName) && !GlobalMapInfoMgr.IsMapExisted(index, theme);
	}

	[Button]
	public void RegisterMapInfo()
	{
		if (ValidationCheck(mapName, index))
			GlobalMapInfoMgr.RegisterMap(mapName, index, mapTheme);
		else throw new System.Exception("Map Already Existed!");
	}
}