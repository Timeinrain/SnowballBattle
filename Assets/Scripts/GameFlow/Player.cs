using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// Player information sharing
/// </summary>
public class Player : MonoBehaviourPun
{
	/// <summary>
	/// For player scoring.
	/// </summary>
	[SerializeField]
	public int kill = 0;
	[SerializeField]
	public int death = 0;
	public Team team;
	public Status status;
	public GameObject instance;
	public enum Status
	{
		Alive = 0,
		Dead = 1,
		Offline = 2,
	}

}