using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[ExecuteInEditMode]
public class GlobalMapInfoMgr : MonoBehaviour
{
	[ShowInInspector]
	public List<Map> readInMaps = new List<Map> { };
	[System.Serializable]
	public struct MapInfo
	{
		[TabGroup("Infos", "Name")]
		[TextArea]
		[ReadOnly] public string mapName;
		//[TabGroup("Infos", "Map")]
		//[TableColumnWidth(40)]
		//[PreviewField(100, Alignment = ObjectFieldAlignment.Left)]
		//[ReadOnly] public Texture map;
		[TabGroup("Infos", "Map")]
		[TableColumnWidth(40)]
		[PreviewField(100, Alignment = ObjectFieldAlignment.Left)]
		[ReadOnly] public GameObject model;
		[TabGroup("Infos", "Descriptions", true)]
		[TextArea]
		[ReadOnly] public string mapDescription;
	}

	[TableList(ShowIndexLabels = true)]
	public List<MapInfo> mapInfos;
	private void Start()
	{
		ReadInMaps();
	}

	[Button]
	void ReadInMaps()
	{
		mapInfos.Clear();
		stringMapMapping.Clear();
		foreach (var mapItem in readInMaps)
		{
			MapInfo temp = new MapInfo
			{
				mapName = mapItem.mapName,
				//map = mapItem.mapImage,
				mapDescription = mapItem.mapDescription,
				model = mapItem.tinyModel,
			};
			mapInfos.Add(temp);
			mapItem.RegisterMapInfo();
		}
	}
	[ShowInInspector]
	[DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.OneLine, KeyLabel = "MapName", ValueLabel = "Encoded As", IsReadOnly = true)]
	public static Dictionary<string, byte> stringMapMapping = new Dictionary<string, byte> { };
	public void GetMapIndex(string mapName, out int index)
	{
		index = stringMapMapping[mapName];
		return;
	}

	public static bool IsMapExisted(string mapName)
	{
		return stringMapMapping.ContainsKey(mapName);
	}
	public static bool IsMapExisted(byte index, MapTheme theme)
	{
		return stringMapMapping.ContainsValue(byte.Parse(theme.GetHashCode().ToString() + index.ToString()));
	}

	public static void RegisterMap(string mapName, byte index, MapTheme mapTheme = MapTheme.Arena)
	{
		string code = "00" + mapTheme.GetHashCode().ToString() + index.ToString();
		byte bCode = byte.Parse(code);
		stringMapMapping.Add(mapName, bCode);
	}

	[Button]
	public void ClearMappingInfos()
	{
		stringMapMapping = new Dictionary<string, byte> { };
	}
}

public enum MapTheme
{
	Nature = 1,
	Festival = 2,
	Party = 3,
	Arena = 4,
	Fantasy = 5,
}

