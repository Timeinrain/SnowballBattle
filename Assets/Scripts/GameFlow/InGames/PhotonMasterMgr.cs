using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Sirenix.OdinInspector;

/// <summary>
/// To fake a global network sync.
/// </summary>
public class PhotonMasterMgr : MonoBehaviourPun
{
	public GameObject BlueTeam;
	public GameObject YellowTeam;
	public GameObject RedTeam;
	public GameObject GreenTeam;
	public GameObject localPlayer;
	public InOutGameRoomInfo roomInfo;
	public static PhotonMasterMgr _Instance;
	public Dictionary<Team, GameObject> teamPosMap;

	public ScoreManager scoreManager;

	public void Awake()
	{
		_Instance = this;
		roomInfo = InOutGameRoomInfo.Instance;
		teamPosMap = new Dictionary<Team, GameObject> { { Team.Blue, BlueTeam }, { Team.Green, GreenTeam }, { Team.Yellow, YellowTeam }, { Team.Red, RedTeam } };

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
	}
}
