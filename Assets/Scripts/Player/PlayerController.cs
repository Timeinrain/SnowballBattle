using Photon.Pun;
using UnityEngine;
/// <summary>
/// 玩家控制脚本，联机仍在测试中
/// </summary>
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : PushableObject
{
	public bool useAbsoluteDirection = false;
	public GameObject playerViewCam;

	CapsuleCollider playerCollider;
	Rigidbody playerRigidbody;
	Animator playerAnimator;
	Character gameLogicHandler;

	[Header("Player Movement Params")]
	[Range(1, 100)]
	public float playerMovingSpeed;
	[Range(10, 1000)]
	public float playerRotationSpeed = 10;
	[Range(1, 50)]
	public float playerOnPushingMovingSpeed;
	[Range(1, 50)]
	public float playerForcedMovingSpeed;
	[Range(0, 90)]
	public float slopeLimit;    // 最大爬坡角

	[Range(1, 50)]
	public float viewFieldRadiance;

	[Header("Push Control Settings")]
	public PushController pushController;
	[Range(1, 100)]
	public float kickSpeed;              // 踢炸弹初速度
	[Range(0.0f, 1.0f)]
	public float rotateTime;             // 踢炸弹前转身时间
	[Range(0.0f, 2.0f)]
	public float kickDelay;              // 踢动画开始到实际踢出炸弹的延迟

	[Header("Throw Settings")]
	[Range(0, 0.5f)]
	public float minThrowSpeedY;         // 垂直方向的最小投掷速度权重
	[Range(0, 0.5f)]
	public float maxThrowSpeedY;         // 垂直方向的最大投掷速度权重

	private float maxThrowDistance;      // 计算得到的最大水平投掷距离

	[Header("Other Settings")]
	[Range(0, 30)]
	public float impulseY;               // 被炸弹炸到之后的冲击力（Y轴力）
	[Range(0, 30)]
	public float impulseXZ;              // 被炸弹炸到之后的冲击力（垂直Y轴平面力）
	public GameObject ice;               // 角色被冰冻时启用

	private Action curState;

	private bool useThrow;               // 启用后投掷会代替踢的动作

	#region 强制移动相关
	private Vector3 forcedMoveDir;
	private Quaternion forcedStartRotation;
	private Quaternion forcedEndRotation;
	private float totolTime;
	private float forcedMoveTimer = 0f;
	private PushableObject forcedMoveObject;
	#endregion

	private Vector3 explosionImpulse = Vector3.zero;

	protected override void Awake()
	{
		base.Awake();

		playerViewCam = GameObject.FindWithTag("MainCamera");
		playerRigidbody = GetComponent<Rigidbody>();
		playerAnimator = GetComponentInChildren<Animator>();
		playerCollider = GetComponent<CapsuleCollider>();

		type = CarryType.Player;
		SetPushable(false);
		ChangeState(Action.FreeRun);
		ice.SetActive(false);

		// 为游戏逻辑处理添加事件
		gameLogicHandler = GetComponent<Character>();
		gameLogicHandler.frozen += Freeze;
		gameLogicHandler.unfrozen += Unfreeze;
		gameLogicHandler.died += Die;
		gameLogicHandler.respawned += Respawn;
		gameLogicHandler.affectedByExplosion += OnAffectedByExplosion;

		/*
		//如果不是本人，就隐藏对应的另一个小地图标识
		if (PhotonNetwork.IsConnected && !photonView.IsMine)
		{
			if (selfMinimap != null) selfMinimap.SetActive(false);
			return;
		}
		*/
	}

    private void Start()
    {
		switch (InOutGameRoomInfo.Instance.currentMap.index)
		{
			case 1:
				{
					//雪地
					useThrow = false;
					break;
				}
			case 2:
				{
					//万圣节
					useThrow = true;
					break;
				}
			case 3:
				{
					//糖果城堡
					useThrow = true;
					break;
				}
		}

		float t = maxThrowSpeedY * kickSpeed / Physics.gravity.magnitude * 2f;
		maxThrowDistance = t * (1f - maxThrowSpeedY) * kickSpeed;
	}

    private void OnDestroy()
	{
		// 注销事件
		gameLogicHandler.frozen -= Freeze;
		gameLogicHandler.unfrozen -= Unfreeze;
		gameLogicHandler.died -= Die;
		gameLogicHandler.respawned -= Respawn;
		gameLogicHandler.affectedByExplosion -= OnAffectedByExplosion;
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
		// 处理强制移动（走向被推动的物体）
		if (curState == Action.ForcedMove)
		{
			if (forcedMoveObject == null)
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
					if (forcedMoveObject != null)
					{
						forcedMoveObject.SetPositionLock(false);    // 解除锁定
						pushController.AttachPushable(forcedMoveObject);
						ChangeState(Action.Pushing);
						forcedMoveObject = null;
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
		// 收到爆炸冲击时不能移动
		if (explosionImpulse.sqrMagnitude > 0.5f)
		{
			playerRigidbody.velocity = explosionImpulse;
			explosionImpulse = Vector3.zero;
			return;
		}
		if (playerRigidbody.velocity.y > 0.5f) return;
			
		Vector3 camForward = new Vector3(playerViewCam.transform.forward.x, 0, playerViewCam.transform.forward.z);
		Vector3 camRight = new Vector3(playerViewCam.transform.right.x, 0, playerViewCam.transform.right.z);
		if (curState == Action.Idle || curState == Action.Pushing || curState == Action.FreeRun)
        {
			Vector3 desiredMove;
			desiredMove = useAbsoluteDirection ? new Vector3(xV, 0, yV) : (xV * camRight + yV * camForward).normalized * playerMovingSpeed;

			// 获取接触表面法线向量
			RaycastHit hitInfo;
			if (Physics.SphereCast(transform.position + new Vector3(0, playerCollider.center.y, 0), playerCollider.radius, Vector3.down, out hitInfo,
							   playerCollider.height / 2f, ~LayerMask.GetMask("Player"), QueryTriggerInteraction.Ignore))
			{
				if (Vector3.Angle(Vector3.up, hitInfo.normal) < slopeLimit)
				{
					Vector3 dir = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;
					desiredMove = dir * playerMovingSpeed;
				}
				playerRigidbody.velocity = desiredMove + new Vector3(0, playerRigidbody.velocity.y, 0);
			}
		}
	}

	/// <summary>
	/// Kick the bomb into specific direction
	/// Or start throwing
	/// </summary>
	public void KickOrThrow()
	{
		if (!CheckAnimatorState("Push Idle", "Push Run")) return;
		if (pushController.GetCarriedType() != PushableObject.CarryType.Bomb) return;

		Vector3 clickPosition;
		if (LocateMousePosition(out clickPosition))
        {
			ChangeState(Action.Kick);

			Vector3 direction = clickPosition - transform.position;
			direction.y = 0f;

			if (!useThrow)
            {
				pushController.Kick(kickSpeed, rotateTime, kickDelay, direction.normalized);
			}
            else
            {
				// 根据点击的远近确定力度
				float throwSpeedY;
				if(direction.magnitude > maxThrowDistance)
                {
					throwSpeedY = maxThrowSpeedY;
                }
                else
                {
					throwSpeedY = (1f - Mathf.Sqrt(1f - 2f * direction.magnitude * Physics.gravity.magnitude / kickSpeed / kickSpeed)) / 2f;
					if (throwSpeedY < minThrowSpeedY)
						throwSpeedY = minThrowSpeedY;
                }

				Vector3 velocityXZ = direction.normalized * kickSpeed * (1f - throwSpeedY);
				Vector3 finalDirection = new Vector3(velocityXZ.x, kickSpeed * throwSpeedY, velocityXZ.z);
				pushController.Kick(kickSpeed, rotateTime, kickDelay, finalDirection.normalized);
			}
		}
	}

	/// <summary>
	/// 开始推动物体或结束推动物体
	/// 如果已经有正在推动的物体，则会放下它，否则会接近最近的一个可被推动的物体尝试开始推动
	/// </summary>
	public void ChangePushState()
	{		
		if (CheckAnimatorState("Push Idle", "Push Run") &&
			pushController.GetCarriedType() == CarryType.Player)   // 只有推动的是冰冻的队友时才能放下
		{
			// 放下正在推动的物体
			StopPushing();
		}
		

		if (CheckAnimatorState("Idle", "Run"))
		{
			PushableObject pushable = pushController.GetPushableInRange();
			if (pushable != null)
			{
				HandleGetBombProcess(pushable);
			}
		}
	}

	private void Freeze(string id)
	{
		ChangeState(Action.Frozen);
		SetPushable(true);   // 冰冻后可以被队友推动
		ice.SetActive(true);
	}

	private void Unfreeze(string id)
	{
		ChangeState(Action.Idle);
		SetPushable(false);
		StopCarrierPushing();
		playerRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
		ice.SetActive(false);
	}

	private void Die(string id)
    {
		ChangeState(Action.Reborn);
		SetPushable(false);
		StopCarrierPushing();
		ice.SetActive(false);
    }

	/// <summary>
	/// 在特定位置复活
	/// </summary>
	/// <param name="id"></param>
	private void Respawn(string id)
    {
		ChangeState(Action.Idle);
		Transform spawnPoint = SpawnManager.Instance.GetSpawnPoint(gameLogicHandler.playerInfo);
		transform.position = spawnPoint.position;
		transform.rotation = spawnPoint.rotation;
		playerRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
	} 


	/// <summary>
	/// 被爆炸波及
	/// </summary>
	/// <param name="sourcePos">炸弹来源</param>
	/// <param name="isFrozenWhenExplosion">当次是否被冰冻</param>
	private void OnAffectedByExplosion(Vector3 sourcePos, bool isFrozenWhenExplosion)
    {
		if (PhotonNetwork.IsConnected && !photonView.IsMine) return;

        if (!isFrozenWhenExplosion)
        {
			Vector3 dir = transform.position - sourcePos;
			dir.y = 0f;
			dir.Normalize();
			dir *= impulseXZ;
			Vector3 impulse = new Vector3(0f, impulseY, 0f) + dir;

			playerAnimator.SetTrigger("Hurt");
			explosionImpulse = impulse;
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
		playerAnimator.SetBool("IsKick", curState == Action.Kick && !useThrow);
		playerAnimator.SetBool("IsPushing", curState == Action.Pushing);
		playerAnimator.SetBool("IsFrozen", curState == Action.Frozen);
		playerAnimator.SetBool("IsDead", curState == Action.Reborn);
		playerAnimator.SetBool("IsThrow", curState == Action.Kick && useThrow);
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
		if (info.IsName("Idle") && curState == Action.FillingCannon)
		{
			ResetStateIfNotConstrained();
		}
	}

	private void HandleGetBombProcess(PushableObject pushable)
	{
		ChangeState(Action.ForcedMove);
		forcedMoveObject = pushable;
		forcedMoveObject.SetPositionLock(true);   // 在此过程中被移动的物体锁定
		Vector3 bombPosition = pushable.transform.position;

		// 人物旋转到可以拿起炸弹的方向
		Vector3 bombDir = bombPosition - transform.position;
		bombDir.y = 0f;
		bombDir.Normalize();
		forcedStartRotation = transform.rotation;
		forcedEndRotation = Quaternion.LookRotation(bombDir, Vector3.up);

		// 计算旋转之后的搬运点位置
		float carryPointDist = (pushController.bombCarryPoint.position - transform.position).magnitude;
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
		Vector3 dir = new Vector3(playerRigidbody.velocity.x, 0, playerRigidbody.velocity.z);
		const float minRotateThreshold = 0.1f;
		if (dir.sqrMagnitude > minRotateThreshold * minRotateThreshold)
		{
			Quaternion targetRotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
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
		if (!CheckStateConstrainted())
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
	/// 判断当前动画是否包含给定状态任意一个
	/// </summary>
	/// <param name="states"></param>
	/// <returns></returns>
	private bool CheckAnimatorState(params string[] states)
	{
		AnimatorStateInfo info = playerAnimator.GetCurrentAnimatorStateInfo(0);
		foreach (var s in states)
		{
			if (info.IsName(s))
				return true;
		}
		return false;
	}

	public void StopPushing()
	{
		pushController.DetachCurrentPushing();
		ResetStateIfNotConstrained();
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
				curState = Action.Pushing;
				break;
			case Action.Frozen:
				StopPushing();
				curState = Action.Frozen;
				break;
			case Action.Kick:
				curState = Action.Kick;
				break;
			case Action.FillingCannon:
				curState = Action.FillingCannon;
				break;
			case Action.Reborn:
				StopPushing();
				curState = Action.Reborn;
				break;
			case Action.ForcedMove:
				curState = Action.ForcedMove;
				break;
		}
	}

	public Action GetCurrentState()
	{
		return curState;
	}

	/// <summary>
	/// 根据鼠标位置获得一个三维坐标
	/// </summary>
	/// <param name="position">返回的坐标</param>
	/// <returns>鼠标点击位置是否合法</returns>
	private bool LocateMousePosition(out Vector3 position)
    {
		// 用于显示场景的画布大小
		const float rawImageWidth = 1920f;
		const float rawImageHeight = 1080f;

		Vector3 mouse = Input.mousePosition;
		Vector3 newMousePos = new Vector3(
			mouse.x / Screen.width * rawImageWidth,
			mouse.y / Screen.width * rawImageWidth + (rawImageWidth - rawImageHeight) / 2f,
			mouse.z);

		Ray ray = Camera.main.ScreenPointToRay(newMousePos);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, 1000))
		{
			position = new Vector3(hit.point.x, hit.point.y, hit.point.z);
			return true;
		}
		else
		{
			position = Vector3.up;
			return false;
		}
	}

    private void OnValidate()
    {
		if (maxThrowSpeedY < minThrowSpeedY) maxThrowSpeedY = minThrowSpeedY;
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
	Kick = 4,           // 包括踢和投掷
	FillingCannon = 5,
	Reborn = 6,
	ForcedMove = 7,     // 强制移动
}