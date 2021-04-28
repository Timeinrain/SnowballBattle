using core.zqc.bombs;
using Photon.Pun;
using System.Collections;
using UnityEngine;
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
	public float viewFieldRadiance;

	[Header("Bomb Control Settings")]
	public BombController bombController;
	[Range(1, 100)]
	public float kickSpeed;              // 踢炸弹初速度
	[Range(0.0f, 2.0f)]
	public float preWaitTime = 0.1f;     // 等待步行动画结束的时间
	[Range(0.0f, 2.0f)]
	public float rotateTime = 0.2f;      // 为拿到炸弹旋转人物的动画时间
	[Range(0.0f, 2.0f)]
	public float kickDelay;              // 踢动画开始到实际踢出炸弹的延迟

	[Header("Map Settings")]
	/// <summary>
	/// Indicator in minimap
	/// </summary>
	public GameObject selfMinimap;
	/// <summary>
	/// Indicator in minimap
	/// </summary>
	public GameObject otherMinimap;

	private Action curState;

	void Awake()
	{
		//如果不是本人，就隐藏对应的另一个小地图标识
		if (PhotonNetwork.IsConnected && !photonView.IsMine)
		{
			if (selfMinimap != null) selfMinimap.SetActive(false);
			return;
		}
		if (otherMinimap != null)
			otherMinimap.SetActive(false);
		playerRigidbody = GetComponent<Rigidbody>();
		playerAnimator = GetComponentInChildren<Animator>();
		playerInfoInstance = GetComponent<Player>();

		curState = Action.FreeRun;
	}

	/// <summary>
	/// For independent freshment.
	/// </summary>
	private void Update()
	{
		if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
		UpdateAnimatorParams();
		HandleStateMachine();
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
		if (curState == Action.Idle || curState == Action.Pushing || curState == Action.FreeRun)
			playerRigidbody.velocity = new Vector3(xV, 0, yV).normalized * playerMovingSpeed + new Vector3(0, playerRigidbody.velocity.y, 0);
	}

	/// <summary>
	/// Kick the bomb into specific direction
	/// </summary>
	public void Kick()
	{
		curState = Action.Kick;
		bombController.Kick(kickSpeed, kickDelay);
	}

	/// <summary>
	/// Try to start pushing if there is a bomb nearby
	/// </summary>
	public void StartPush()
    {
		if (curState == Action.Pushing) return;
		Bomb bomb = bombController.GetBombInRange();
        if (bomb != null)
        {
			StartCoroutine(HandleGetBombProcess(bomb));
        }
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
		playerAnimator.SetBool("IsKick", curState == Action.Kick);
		playerAnimator.SetBool("IsPushing", curState == Action.Pushing);
		playerAnimator.SetBool("IsFiring", curState == Action.Fire);
		playerAnimator.SetBool("InAnimation", curState == Action.InAnimation);
	}

	/// <summary>
	/// 处理一些需要动画事件来判断开始和结束的状态
	/// </summary>
	private void HandleStateMachine()
    {
		AnimatorStateInfo info = playerAnimator.GetCurrentAnimatorStateInfo(0);
        if (info.IsName("Idle") && curState == Action.Kick)
        {
			curState = Action.FreeRun;
        }
    }


	private IEnumerator HandleGetBombProcess(Bomb bomb)
    {
		curState = Action.InAnimation;
		Vector3 bombPosition = bomb.transform.position;

		yield return new WaitForSeconds(preWaitTime);

		// 人物旋转到可以拿起炸弹的方向
		Vector3 bombDir = bombPosition - transform.position;
		bombDir.y = 0f;
		bombDir.Normalize();
		Quaternion startRotation = transform.rotation;
		Quaternion endRotation = Quaternion.LookRotation(bombDir, Vector3.up);
		for (float timer = 0f; timer <= rotateTime; timer += Time.fixedDeltaTime)
		{
			transform.rotation = Quaternion.Slerp(startRotation, endRotation, timer / rotateTime);
			yield return new WaitForFixedUpdate();
		}
		transform.rotation = endRotation;

		curState = Action.FreeRun;

		if (bomb != null)
        {
			bombController.AttachBomb(bomb);
			curState = Action.Pushing;
		}
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

	public void SetState(Action action)
    {
		curState = action;
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
	InAnimation = 7,   // 前摇或后摇时间内
}