using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;

/// <summary>
/// To fake a global network sync.
/// </summary>
public class PhotonMasterMgr : MonoBehaviourPun
{
	public GameObject RedTeam;
	public GameObject GreenTeam;
	public GameObject localPlayer;
	public InOutGameRoomInfo roomInfo;
	[ShowInInspector]
	public static PhotonMasterMgr _Instance;
	public Dictionary<Team, GameObject> teamPosMap;

	public ScoreManager scoreManager;

	public void OnEnable()
	{
		if (_Instance == null)
			_Instance = this;
		roomInfo = InOutGameRoomInfo.Instance;
		teamPosMap = new Dictionary<Team, GameObject> { { Team.Green, GreenTeam }, { Team.Red, RedTeam } };
		// 生成角色被移入Start()，防止与其他初始化冲突
	}


	public void Start()
	{
		//生成角色
		Player playerInfo = roomInfo.GetPlayerByName(PhotonNetwork.LocalPlayer.NickName);

		Transform spawnPoint = SpawnManager.Instance.GetSpawnPoint(playerInfo);
		GameObject go = PhotonNetwork.Instantiate("Mochi", spawnPoint.position, spawnPoint.rotation, 0);

		go.tag = playerInfo.team.ToString() + "Team";
		go.GetComponent<Character>().id = playerInfo.playerId;
		go.GetComponent<PlayerInfoSync>().SetInfo();
		go.GetComponent<PlayerInfoSync>().playerName.text = playerInfo.playerId;
		scoreManager = FindObjectOfType<ScoreManager>();
	}

	[PunRPC]
	public void SaveData()
	{
		InOutGameRoomInfo.Instance.SaveData();
	}

	[PunRPC]
	public void ReturnToRoom()
	{
		InOutGameRoomInfo.Instance.inRoomPlayerInfos.Clear();
		SceneManager.LoadScene(0);
	}

	public void EndGame()
	{
		if (scoreManager.GetTeam(true) == InOutGameRoomInfo.Instance.GetPlayerByName(InOutGameRoomInfo.Instance.localPlayerId).team)
		{
			InOutGameRoomInfo.Instance.isVictory = true;
		}
		else
		{
			InOutGameRoomInfo.Instance.isVictory = false;
		}
	}
}
