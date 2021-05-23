using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviourPun
{
	/// <summary>
	/// Singleton
	/// </summary>
	public static GameManager Instance;

	//All team information
	[SerializeField]
	public List<TeamInfoMgr> teams;

	/// <summary>
	/// Game Time
	/// </summary>
	public float totalTime;
	public float currentTime;

	/// <summary>
	/// maxPlayers
	/// </summary>
	public int maxPlayers;


	private void Start()
	{
		Instance = this;
		StartCoroutine(WaitForAllPlayersToJoin());
	}

	/// <summary>
	/// When Ending Game :
	/// 1.Sort the ranking list
	/// 2.Call the ranking panel
	/// 3.Return to room
	/// </summary>
	public void EndGame()
	{
		//Sort according to score.
		SortTeamList(teams);
		//Display game outcome.

	}

	/// <summary>
	/// Using quickSort
	/// </summary>
	/// <param name="teamInfoMgrs"></param>
	private void SortTeamList(List<TeamInfoMgr> teamInfoMgrs)
	{
		//todo : filled the sorting func
		Debug.Log("GameManager.cs fill sorting func");


	}

	/// <summary>
	/// Wait for all players to join the room
	/// if any quit or dropped, remove it.
	/// </summary>
	public IEnumerator WaitForAllPlayersToJoin()
	{
		while (true)
		{
			//如果全都进来了，就开始倒计时，开始游戏
			if (PhotonNetwork.CountOfPlayersInRooms == maxPlayers) break;
			else if (Input.GetKey(KeyCode.B)) break;//用于测试开始游戏，后期删除
			else
			{
				foreach (var player in PhotonNetwork.PlayerList)
				{
					Debug.Log(player.ToString() + player.IsMasterClient);
				}
			}
			yield return new WaitForEndOfFrame();
		}
		StartCoroutine(StartGameTimer());
	}

	/// <summary>
	/// Timer before starting game.
	/// </summary>
	/// <returns></returns>
	IEnumerator StartGameTimer()
	{
		//Sync the masterClient's phyx time.
		int sec = 0;
		while (sec < 3)
		{
			sec++;
			yield return new WaitForSeconds(1);
		}
		StartGame();
	}

	/// <summary>
	/// 开始游戏的逻辑，可以行动+进行动作
	/// </summary>
	public void StartGame()
	{
		FindObjectOfType<BombGeneratorOnline>().StartGenerateBomb();
	}



}

public enum GameMode
{
	PVP,
	PVE,
}