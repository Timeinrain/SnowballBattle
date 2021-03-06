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
	public GameObject settlementUI;
	public CountdownDisplay timerUI;
	public float currTime = -10;
	[ShowInInspector]
	public static PhotonMasterMgr _Instance;
	public Dictionary<Team, GameObject> teamPosMap;

	public ScoreManager scoreManager;

	private BombGeneratorOnline infiniteBombGenerator = null;

	public void OnEnable()
	{
		if (_Instance == null)
			_Instance = this;
		roomInfo = InOutGameRoomInfo.Instance;
		teamPosMap = new Dictionary<Team, GameObject> { { Team.Green, GreenTeam }, { Team.Red, RedTeam } };
		StartGameTiming();
		// 生成角色被移入Start()，防止与其他初始化冲突
	}

	public void StartGameTiming()
	{
		StartCoroutine(Timer());
	}

	IEnumerator Timer()
	{
		while (true)
		{
			currTime += Time.deltaTime;
			if (currTime >= 10.0) GameManager.Instance.startGame = true;
			if (PhotonNetwork.LocalPlayer.IsMasterClient)
			{
				if (currTime >= 180)
				{
					InOutGameRoomInfo.Instance.isSettlement = true;
					currTime = 0;
					InOutGameRoomInfo.Instance.ExitGameRound();
					break;
				}
				GameObject generatorGO = GameObject.Find("Infinitive Bomb Generator");
				if (infiniteBombGenerator == null && generatorGO != null)
				{
					infiniteBombGenerator = generatorGO.GetComponent<BombGeneratorOnline>();
				}
				if (infiniteBombGenerator != null)
					infiniteBombGenerator.bombsFallingNumber = (int)(currTime / 90 * 10 + 10);
			}
			if (PhotonNetwork.LocalPlayer.IsMasterClient)
			{
				timerUI.SetTime((int)(180 - currTime));
				photonView.RPC("SyncTimer", RpcTarget.Others, (int)(180 - currTime));
			}
			yield return new WaitForEndOfFrame();
		}
	}

	[PunRPC]
	public void SyncTimer(int time)
	{
		timerUI.SetTime(time);
	}

	public void Start()
	{
		//生成角色
		Player playerInfo = roomInfo.GetPlayerByName(PhotonNetwork.LocalPlayer.NickName);

		Transform spawnPoint = SpawnManager.Instance.GetSpawnPoint(playerInfo);
		GameObject go = PhotonNetwork.Instantiate(InOutGameRoomInfo.Instance.prefabIndex.ToString(), spawnPoint.position, spawnPoint.rotation, 0);

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

	[PunRPC]
	public void StartSettlement()
	{
		bool victory = InOutGameRoomInfo.Instance.isVictory;
		settlementUI.SetActive(true);
		settlementUI.GetComponent<SettlementPanel>().StartSettle(victory);
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
