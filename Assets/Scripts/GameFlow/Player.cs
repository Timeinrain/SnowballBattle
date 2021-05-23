using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// Player information sharing
/// </summary>
public class Player : MonoBehaviourPun
{
	public string playerId;
	public int maxLifeCount = 3;
	public Team team;
	public Status status;
	static public GameObject _Instance;
	public static Dictionary<int, Team> teamIntMapping;
	public void Awake()
	{
		_Instance = gameObject;
		if (teamIntMapping == null)
		{
			teamIntMapping = new Dictionary<int, Team> { { 0, Team.Blue }, { 1, Team.Red }, { 2, Team.Yellow }, { 3, Team.Green } };
		}
	}
	public enum Status
	{
		InGame = 0,
		Out = 1,
		Offline = 2,
	}

	[PunRPC]
	public void InitInfo(string id, int teamIndex)
	{
		playerId = id;
		team = teamIntMapping[teamIndex];
		status = Status.InGame;
	}
}