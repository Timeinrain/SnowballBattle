using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class CreateRoom : MonoBehaviourPun
{
	public Text roomName;
	public byte maxPlayers;
	public void Create()
	{
		FindObjectOfType<NetworkInit>().CreateRoom(roomName.text, maxPlayers);
	}

}
