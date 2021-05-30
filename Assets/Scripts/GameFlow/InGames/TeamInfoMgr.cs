using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// Share Info
/// </summary>
public sealed class TeamInfoMgr : MonoBehaviourPun
{
	[Header("Team Infomation")]
	[SerializeField]
	public Team currentTeam;
	[SerializeField]
	public List<Player> teamMates;
	public int currentTeamScore = 0;

	[Header("Infomation Sharing")]
	public List<Player> aliveTeamMates;
}


/// <summary>
/// Instantly set 4 teams.
/// </summary>
public enum Team
{
	Green = 0,
	Red = 1,
	Null = 2,
}