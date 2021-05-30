using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Sirenix.OdinInspector;
/// <summary>
/// ===============
/// 要做的：
/// 1.首先自己的名字同步了
/// 
/// 2.把其他玩家的名字同步
/// 
/// 其他玩家名字同步方法：
/// 
/// 
/// </summary>
public class PlayerInfoSync : MonoBehaviourPun
{
	public Character thisCharacter;

	public GameObject teamMiniMap;

	public GameObject playerModel;

	[Header("Models")]
	public GameObject halloweenHat;
	public GameObject sailorHat;

	[Header("Materials")]
	public Material defaultMatRed;
	public Material defaultMatGreen;
	public Material halloweenMatRed;
	public Material halloweenMatGreen;
	public Material sailorMatRed;
	public Material sailorMatGreen;

	public GameObject lightObject;

	public Text playerName;

	[FoldoutGroup("Sprites")]
	public Material red;
	[FoldoutGroup("Sprites")]
	public Material green;

	private void OnEnable()
	{
		SetInfo();
		FitToMap(InOutGameRoomInfo.Instance.currentMap);
	}

	private void FitToMap(Map map)
	{
		switch (map.index)
		{
			case 1:
				{
					break;
				}
			case 2:
				{
					//万圣节
					lightObject.SetActive(true);
					playerModel.GetComponent<SkinnedMeshRenderer>().material = defaultMatRed;
					break;
				}
			case 3:
				{
					
					break;
				}
		}
	}

	public void SetWorldUIInfo()
	{
		thisCharacter = GetComponent<Character>();
		thisCharacter.playerInfo = InOutGameRoomInfo.Instance.GetPlayerByName(InOutGameRoomInfo.Instance.localPlayerId);
		Dictionary<Team, Material> teamSpriteMap = new Dictionary<Team, Material> { { Team.Green, green }, { Team.Red, red } };
		GameObject teamObject = PhotonMasterMgr._Instance.teamPosMap[thisCharacter.playerInfo.team];
		transform.SetParent(teamObject.transform);
		thisCharacter.playerInfo.Instance = gameObject;
		teamMiniMap.GetComponent<MeshRenderer>().material = teamSpriteMap[thisCharacter.playerInfo.team];
		playerName.text = thisCharacter.id;
	}

	public void SetInfo()
	{
		if (photonView.IsMine)
			SetWorldUIInfo();
		else
		{
			thisCharacter = GetComponent<Character>();
			thisCharacter.id = photonView.Owner.NickName;
			Player playerInfo = InOutGameRoomInfo.Instance.GetPlayerByName(photonView.Owner.NickName);
			thisCharacter.playerInfo = playerInfo;
			Dictionary<Team, Material> teamSpriteMap = new Dictionary<Team, Material> { { Team.Green, green },{ Team.Red, red } };
			GameObject teamObject = PhotonMasterMgr._Instance.teamPosMap[thisCharacter.playerInfo.team];
			transform.SetParent(teamObject.transform);
			thisCharacter.playerInfo.Instance = gameObject;
			teamMiniMap.GetComponent<MeshRenderer>().material = teamSpriteMap[thisCharacter.playerInfo.team];
			playerName.text = thisCharacter.id;
		}
	}
}
