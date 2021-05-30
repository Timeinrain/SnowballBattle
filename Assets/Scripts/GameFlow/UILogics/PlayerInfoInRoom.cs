using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Sirenix.OdinInspector;

public class PlayerInfoInRoom : MonoBehaviourPun
{
	[ReadOnly]
	public string id;
	[PreviewField]
	public GameObject masterIndicator;
	public Text playerName;
	public Team team = Team.Green;
	public Image teamImg;
	#region sprites
	[FoldoutGroup("Sprites")]
	public Sprite red;
	[FoldoutGroup("Sprites")]
	public Sprite green;
	#endregion

	public int inRoomPosIndex;

	[ShowInInspector]
	[ReadOnly]
	public static Dictionary<Team, Sprite> teamIcon = null;

	Animator anim;
	private void Awake()
	{
		anim = GetComponent<Animator>();
		teamIcon = new Dictionary<Team, Sprite> { { Team.Green, green }, { Team.Red, red } };
	}

	private void OnEnable()
	{
		anim.SetTrigger("FlashIn");
	}

	/// <summary>
	/// Update TeamIcon and playerName.
	/// </summary>
	public void UpdateInfo()
	{
		playerName.text = id;
		UpdateTeamInfo();
	}

	/// <summary>
	/// Animate the Master Client FX
	/// </summary>
	public void AnimateMasterClient()
	{
		masterIndicator.SetActive(true);
		masterIndicator.GetComponent<Animator>().SetTrigger("Enter");
	}

	/// <summary>
	/// When not-local client exit the room.
	/// Play the Exit animation.
	/// </summary>
	public void ExitRoom()
	{
		anim.SetTrigger("FlashOut");
		if (!PhotonNetwork.LocalPlayer.IsMasterClient)
			StartCoroutine(DestroySelf());
	}

	IEnumerator DestroySelf()
	{
		yield return new WaitForSeconds(0.5f);
		Destroy(gameObject);
	}

	[Button]
	private void UpdateTeamInfo(bool isInit = false)
	{
		if (teamIcon == null)
			teamIcon = new Dictionary<Team, Sprite> { { Team.Green, green }, { Team.Red, red } };
		teamImg.sprite = teamIcon[team];
		if (!isInit)
			teamImg.GetComponent<Animator>().SetTrigger("TeamSwitched");
	}

}
