using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// Share Information about a room info
/// </summary>
public class RoomConfig
{
	public string mapName;
	public string roomName;
	public byte maxPlayers;
	public byte currentPlayers;
	public string gameMode;
}
