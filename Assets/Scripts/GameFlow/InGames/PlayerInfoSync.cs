using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Sirenix.OdinInspector;
/// <summary>
/// ===============
/// Ҫ���ģ�
/// 1.�����Լ�������ͬ����
/// 
/// 2.��������ҵ�����ͬ��
/// 
/// �����������ͬ��������
/// 
/// 
/// </summary>
public class PlayerInfoSync : MonoBehaviourPun
{
	public Character thisCharacter;

	public GameObject teamMiniMap;

	public GameObject playerModel;

	[Header("Materials")]
	public Material halloweenMat;
	public Material snowMat;
	public Material sweetMat;

	public GameObject lightObject;

	public Text playerName;

	[FoldoutGroup("Sprites")]
	public Material red;
	[FoldoutGroup("Sprites")]
	public Material yellow;
	[FoldoutGroup("Sprites")]
	public Material green;
	[FoldoutGroup("Sprites")]
	public Material blue;

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
					//ѩ��
					lightObject.SetActive(false);
					playerModel.GetComponent<SkinnedMeshRenderer>().material = snowMat;
					break;
				}
			case 2:
				{
					//��ʥ��
					lightObject.SetActive(true);
					playerModel.GetComponent<SkinnedMeshRenderer>().material = halloweenMat;
					break;
				}
		}
	}

	public void SetWorldUIInfo()
	{
		thisCharacter = GetComponent<Character>();
		thisCharacter.playerInfo = InOutGameRoomInfo.Instance.GetPlayerByName(InOutGameRoomInfo.Instance.localPlayerId);
		Dictionary<Team, Material> teamSpriteMap = new Dictionary<Team, Material> { { Team.Blue, blue }, { Team.Green, green }, { Team.Yellow, yellow }, { Team.Red, red } };
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
			Dictionary<Team, Material> teamSpriteMap = new Dictionary<Team, Material> { { Team.Blue, blue }, { Team.Green, green }, { Team.Yellow, yellow }, { Team.Red, red } };
			GameObject teamObject = PhotonMasterMgr._Instance.teamPosMap[thisCharacter.playerInfo.team];
			transform.SetParent(teamObject.transform);
			thisCharacter.playerInfo.Instance = gameObject;
			teamMiniMap.GetComponent<MeshRenderer>().material = teamSpriteMap[thisCharacter.playerInfo.team];
			playerName.text = thisCharacter.id;
		}
	}
}
