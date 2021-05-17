using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Map : ScriptableObject
{
	public string mapInfoString
	{
		get
		{
			return mapType.ToString();
		}
		set
		{
			mapType = stringMapMapping[value];
		}
	}

	public MapType mapType = MapType.SnowMountain;
	public enum MapType
	{
		SnowMountain = 0,
		Halloween = 1,
	}

	static Dictionary<string, MapType> stringMapMapping = new Dictionary<string, MapType>
	{
		{ "ѩɽ�Ҷ�" , MapType.SnowMountain},
		{ "���ձȺ���" , MapType.Halloween },
	};

	public void OnValuaChanged(string targetMap)
	{
		mapInfoString = targetMap;
	}
}