using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// To fake a global network sync.
/// </summary>
public class PhotonMasterMgr : MonoBehaviourPun
{
	public GameObject BlueTeam;
	public GameObject YellowTeam;
	public GameObject RedTeam;
	public GameObject GreenTeam;
	public InOutGameRoomInfo roomInfo;
	public static Dictionary<Team, GameObject> teamPosMap;
	public void OnEnable()
	{
		roomInfo = FindObjectOfType<InOutGameRoomInfo>();
		teamPosMap = new Dictionary<Team, GameObject> { { Team.Blue, BlueTeam }, { Team.Green, GreenTeam }, { Team.Yellow, YellowTeam }, { Team.Red, RedTeam } };
		GameObject go = PhotonNetwork.Instantiate("Mochi", new Vector3(Random.Range(-1, 1), 3, Random.Range(-1, 1)), Quaternion.identity, 0);
		Player playerInfo = roomInfo.GetPlayerByName(PhotonNetwork.LocalPlayer.NickName);
		go.tag = playerInfo.team.ToString() + "Team";
		go.GetComponent<Character>().id = playerInfo.playerId;
		GameObject teamObject = teamPosMap[playerInfo.team];
		go.transform.SetParent(teamObject.transform);
	}

}
