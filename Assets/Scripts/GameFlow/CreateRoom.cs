using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class CreateRoom : MonoBehaviourPun
{
	public Text roomName;
	public byte maxPlayers;
	public GameMode gameMode;
	public void Create()
	{
		FindObjectOfType<NetWorkMgr>().CreateRoom(roomName.text, maxPlayers, gameMode);
	}

}
