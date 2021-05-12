using core.zqc.bombs;
using Photon.Pun;
using UnityEditor;
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
	public float playerForcedMovingSpeed;

	[Range(1, 50)]
	public float viewFieldRadiance;

	[Header("Bomb Control Settings")]
	public BombController bombController;
	[Range(1, 100)]
	public float kickSpeed;              // 踢炸弹初速度
	[Range(0.0f, 2.0f)]
	public float kickDelay;              // 踢动画开始到实际踢出炸弹的延迟

	[Header("Other Animation Settings")]
	[Range(0.0f, 10.0f)]
	public float fireDelay;              // 点火动画开始到实际将炸弹送出的延迟

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
	private Cannon nearbyCannon = null;

    #region 强制移动相关
	private Vector3 forcedMoveDir;
	private Quaternion forcedStartRotation;
	private Quaternion forcedEndRotation;
	private float totolTime;
	private float forcedMoveTimer = 0f;
	private Bomb carriedBomb;
    #endregion

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

		ChangeState(Action.FreeRun);
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

    private void FixedUpdate()
    {
        // 处理强制移动
		if (curState == Action.ForcedMove)
        {
			if (carriedBomb == null)
            {
				ResetStateIfNotConstrained();
				forcedMoveTimer = 0f;
            }
            else
            {
				if (forcedMoveTimer < totolTime)
				{
					playerRigidbody.velocity = forcedMoveDir * playerForcedMovingSpeed;
					playerRigidbody.MoveRotation(Quaternion.Slerp(forcedStartRotation, forcedEndRotation, forcedMoveTimer * playerRotationSpeed));
					forcedMoveTimer += Time.fixedDeltaTime;
				}
				else
				{
					forcedMoveTimer = 0f;
					playerRigidbody.velocity = Vector3.zero;
					if (carriedBomb != null)
					{
						carriedBomb.SetPositionLock(false);
						bombController.AttachBomb(carriedBomb);
						ChangeState(Action.Pushing);
					}
					else
					{
						ResetStateIfNotConstrained();
					}
				}
			}
        }
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
		if (!CheckAnimatorState("Push Idle", "Push Run")) return;
		ChangeState(Action.Kick);
		bombController.Kick(kickSpeed, kickDelay,transform.forward);
	}

	/// <summary>
	/// Try to start pushing if there is a bomb nearby
	/// </summary>
	public void StartPush()
    {
		if (!CheckAnimatorState("Idle", "Run")) return;
		Bomb bomb = bombController.GetBombInRange();
        if (bomb != null)
        {
			HandleGetBombProcess(bomb);
        }
    }

	/// <summary>
	/// Try to fill a cannon if there is a cannon nearby
	/// </summary>
	public void Fire()
    {
		if (!CheckAnimatorState("Push Idle", "Push Run")) return;
		if (nearbyCannon != null)
        {
			ChangeState(Action.Fire);
			Bomb bomb = bombController.GetCarriedBomb();
			bomb.StopExplosionCountdown();    // 炸弹不会在填入炮台动画中爆炸
			nearbyCannon.FillBomb(bomb, fireDelay);
        }
    }

	public void Freeze()
    {
		ChangeState(Action.Frozen);
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
		playerAnimator.SetBool("IsFrozen", curState == Action.Frozen);
		//playerAnimator.SetBool("IsForcedMove", curState == Action.ForcedMove);
		if(carriedBomb == null && !CheckStateAnimation())
        {
			ResetStateIfNotConstrained();
        }
	}

	/// <summary>
	/// 处理一些需要动画事件来判断开始和结束的状态
	/// </summary>
	private void HandleStateMachine()
    {
		AnimatorStateInfo info = playerAnimator.GetCurrentAnimatorStateInfo(0);
        if (info.IsName("Idle") && curState == Action.Kick)
        {
			ResetStateIfNotConstrained();
        }
		if (info.IsName("Idle") && curState == Action.Fire)
        {
			ResetStateIfNotConstrained();
		}
    }

	private void HandleGetBombProcess(Bomb bomb)
    {
		ChangeState(Action.ForcedMove);
		carriedBomb = bomb;
		carriedBomb.SetPositionLock(true);
		Vector3 bombPosition = bomb.transform.position;

		// 人物旋转到可以拿起炸弹的方向
		Vector3 bombDir = bombPosition - transform.position;
		bombDir.y = 0f;
		bombDir.Normalize();
		forcedStartRotation = transform.rotation;
		forcedEndRotation = Quaternion.LookRotation(bombDir, Vector3.up);

		// 计算旋转之后的搬运点位置
		float carryPointDist = (bombController.carryPoint.position - transform.position).magnitude;
		Vector3 rotatedCarryPoint = bombDir * carryPointDist + transform.position;
		Vector3 dist = bombPosition - rotatedCarryPoint;
		dist.y = 0f;
		forcedMoveDir = dist.normalized;

		float rotateTime = 1.0f / playerRotationSpeed;
		float moveTime = dist.magnitude / playerForcedMovingSpeed;
		totolTime = rotateTime > moveTime ? rotateTime : moveTime;
		forcedMoveTimer = 0f;
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
		//todo
	}

	/// <summary>
	/// 如果当前角色非受限，则重置为FreeRun
	/// </summary>
	private void ResetStateIfNotConstrained()
    {
		if(!CheckStateConstrainted())
        {
			ChangeState(Action.FreeRun);
        }
    }

	/// <summary>
	/// 确认当前状态是否为受限的状态
	/// </summary>
	/// <returns></returns>
	private bool CheckStateConstrainted()
    {
        switch (curState)
        {
			case Action.Frozen:
			case Action.Reborn:
				return true;
        }
		return false;
    }

	/// <summary>
	/// 确认当前状态是否为动画状态，这些状态在动画结束之后会自动切换
	/// <see cref="HandleStateMachine"></see>
	/// </summary>
	/// <returns></returns>
	private bool CheckStateAnimation()
    {
        switch (curState)
        {
			case Action.Kick:
			case Action.Fire:
				return true;
        }
		return false;
    }

	/// <summary>
	/// 判断当前动画是否包含给定状态任意一个
	/// </summary>
	/// <param name="states"></param>
	/// <returns></returns>
	private bool CheckAnimatorState(params string[] states)
    {
		AnimatorStateInfo info = playerAnimator.GetCurrentAnimatorStateInfo(0);
		foreach(var s in states)
        {
			if (info.IsName(s))
				return true;
        }
		return false;
	}

	/// <summary>
	/// Set the player's belonged team.
	/// </summary>
	/// <param name="team"></param>
	public void SetTeam(Team team)
	{
		playerInfoInstance.team = team;
	}

	public void ChangeState(Action action)
    {
        switch (action)
        {
            case Action.Idle:
				curState = Action.Idle;
                break;
            case Action.FreeRun:
				curState = Action.FreeRun;
                break;
            case Action.Pushing:
				Debug.Log("Changed to Pushing");
				curState = Action.Pushing;
                break;
            case Action.Frozen:
				bombController.DetachCurrentBomb();
				carriedBomb = null;
				curState = Action.Frozen;
				break;
            case Action.Kick:
				Debug.Log("Changed to Kick");
				curState = Action.Kick;
                break;
            case Action.Fire:
				curState = Action.Fire;
                break;
            case Action.Reborn:
				bombController.DetachCurrentBomb();
				carriedBomb = null;
				curState = Action.Reborn;
                break;
            case Action.ForcedMove:
				Debug.Log("Changed to ForcedMove");
				curState = Action.ForcedMove;
                break;
        }
    }

    public void AddNearbyCannon(Cannon cannon)
    {
		nearbyCannon = cannon;
    }

	public void RemoveNearbyCannon()
    {
		nearbyCannon = null;
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
	ForcedMove = 7,     // 强制移动
}