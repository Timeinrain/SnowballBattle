using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Player information sharing
/// </summary>
public class Player
{
	public string playerId;
	public int maxLifeCount = 3;
	public Team team;
	public Status status;
	//public static Dictionary<int, Team> teamIntMapping = new Dictionary<int, Team> { { 0, Team.Blue }, { 1, Team.Red }, { 2, Team.Yellow }, { 3, Team.Green } };
	public enum Status
	{
		InGame = 0,
		Out = 1,
		Offline = 2,
	}
}