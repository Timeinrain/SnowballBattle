using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// To carry any operation concerned team
/// </summary>
public sealed class TeamController : MonoBehaviour
{
	TeamInfoMgr _info;

	private void Start()
	{
		_info = GetComponent<TeamInfoMgr>();
	}

	/// <summary>
	/// When starting or restarting game. Call this to add all teammate to alive list.
	/// </summary>
	/// <param name="teamMatesValues"></param>
	public void RefreshTeamMates(List<Player> teamMatesValues)
	{
		_info.aliveTeamMates = teamMatesValues;
	}

	/// <summary>
	/// When a teammate die, remove from alive list.
	/// </summary>
	/// <param name="teamMate"></param>
	public void TeamMateDeath(Player teamMate)
	{
		if (_info.aliveTeamMates.Contains(teamMate))
		{
			_info.aliveTeamMates.Remove(teamMate);
			print(teamMate + " has been removed from alive list.");
		}
	}

	/// <summary>
	/// When a teammate reborn, rejoin alive list.
	/// </summary>
	/// <param name="teamMate"></param>
	public void TeamMateReborn(Player teamMate)
	{
		if (!_info.aliveTeamMates.Contains(teamMate))
		{
			_info.aliveTeamMates.Add(teamMate);
			print(teamMate + " reborn.");
		}
	}

}
