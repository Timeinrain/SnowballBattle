using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
/// <summary>
/// 玩家控制脚本，联机仍在测试中
/// </summary>
public class PlayerController : MonoBehaviourPun
{

	Rigidbody playerRigidbody;
	Animator playerAnimator;
	Player playerInfoInstance;

	[Header("Player Movement Params")]
	[Range(1, 100)]
	public float playerMovingSpeed;
	[Range(10, 1000)]
	public float playerRotationSpeed = 10;
	[Range(1, 50)]
	public float playerOnPushingMovingSpeed;
	[Range(1, 50)]
	public float kickForce;
	[Range(1, 50)]
	public float viewFieldRadiance;

	/// <summary>
	/// Indicator in minimap
	/// </summary>
	public GameObject selfMinimap;
	/// <summary>
	/// Indicator in minimap
	/// </summary>
	public GameObject otherMinimap;

	void Awake()
	{
		//如果不是本人，就隐藏对应的另一个小地图标识
		if (PhotonNetwork.IsConnected && !photonView.IsMine)
		{
			selfMinimap.SetActive(false);
			return;
		}
		otherMinimap.SetActive(false);
		playerRigidbody = GetComponent<Rigidbody>();
		playerAnimator = GetComponentInChildren<Animator>();
		playerInfoInstance = GetComponent<Player>();
	}

	/// <summary>
	/// For independent freshment.
	/// </summary>
	private void Update()
	{
		if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
		UpdateAnimatorParams();
		RefreshAnimation();
		UpdateVisibleEnemyPos();
	}

	/// <summary>
	/// Control Simple Move
	/// </summary>
	/// <param name="xV"> Horizontal </param>
	/// <param name="yV"> Vertical </param>
	public void Move(float xV, float yV)
	{
		playerRigidbody.velocity = new Vector3(xV, 0, yV).normalized * playerMovingSpeed + new Vector3(0, playerRigidbody.velocity.y, 0);

	}

	/// <summary>
	/// Kick the bomb into specific direction
	/// </summary>
	/// <param name="bombInstance"> The bomb instance </param>
	/// <param name="dir"> Specified Direction </param>
	public void Kick(GameObject bombInstance, Vector3 dir)
	{

	}

	/// <summary>
	/// Action Responce
	/// </summary>
	/// <param name="action"> Action Type </param>
	public void PlayerAction(Action action)
	{
		switch (action)
		{
			case Action.Idle:
				{
					break;
				}
		}
	}

	/// <summary>
	/// Refresh the animation states
	/// </summary>
	public void UpdateAnimatorParams()
	{
		playerAnimator.SetFloat("MovingSpeed", (new Vector2(playerRigidbody.velocity.x, playerRigidbody.velocity.z)).magnitude);

	}

	/// <summary>
	/// Refresh the character's facing dir and so on.
	/// </summary>
	private void RefreshAnimation()
	{
		Vector3 dir = new Vector3(playerRigidbody.velocity.x, 0, playerRigidbody.velocity.z).normalized;
		if (dir.magnitude != 0)
		{
			Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);
			Quaternion lerp = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * playerRotationSpeed);
			playerRigidbody.MoveRotation(lerp);
		}
	}

	/// <summary>
	/// Set to specific animation.
	/// </summary>
	/// <param name="flag"></param>
	public void SetAnimationFlag(bool flag)
	{
		//todo: fill
	}

	/// <summary>
	/// To display the visible enemy's position in the minimap.
	/// </summary>
	private void UpdateVisibleEnemyPos()
	{

	}

	/// <summary>
	/// Set the player's belonged team.
	/// </summary>
	/// <param name="team"></param>
	public void SetTeam(Team team)
	{
		playerInfoInstance.team = team;
	}
}

/// <summary>
/// The Action Types In Total
/// </summary>
public enum Action
{
	Idle = 0,
	FreeRun = 1,
	Pushing = 2,
	Frozen = 3,
	Kick = 4,
	Fire = 5,
	Reborn = 6,
}