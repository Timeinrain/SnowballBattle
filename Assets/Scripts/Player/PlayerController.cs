using core.zqc.bombs;
using Photon.Pun;
using UnityEngine;
/// <summary>
/// ��ҿ��ƽű����������ڲ�����
/// </summary>
public class PlayerController : PushableObject
{
	public bool useAbsoluteDirection = false;
	public GameObject playerViewCam;

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

	[Range(1, 50)]
	public float viewFieldRadiance;

	[Header("Push Control Settings")]
	public PushController pushController;
	[Range(1, 100)]
	public float kickSpeed;              // ��ը�����ٶ�
	[Range(0.0f, 2.0f)]
	public float kickDelay;              // �߶�����ʼ��ʵ���߳�ը�����ӳ�

	[Header("Other Settings")]
	public GameObject ice;               // ��ɫ������ʱ����

	private Action curState;
	// private Cannon nearbyCannon;

	#region ǿ���ƶ����
	private Vector3 forcedMoveDir;
	private Quaternion forcedStartRotation;
	private Quaternion forcedEndRotation;
	private float totolTime;
	private float forcedMoveTimer = 0f;
	private PushableObject forcedMoveObject;
	#endregion

	protected override void Awake()
	{
		base.Awake();

		playerViewCam = GameObject.FindWithTag("MainCamera");
		playerRigidbody = GetComponent<Rigidbody>();
		playerAnimator = GetComponentInChildren<Animator>();

		type = CarryType.Player;
		SetPushable(false);
		ChangeState(Action.FreeRun);
		ice.SetActive(false);

		// Ϊ��Ϸ�߼���������¼�
		gameLogicHandler = GetComponent<Character>();
		gameLogicHandler.frozen += Freeze;
		gameLogicHandler.unfrozen += Unfreeze;
		gameLogicHandler.died += Die;
		gameLogicHandler.respawned += Respawn;

		/*
		//������Ǳ��ˣ������ض�Ӧ����һ��С��ͼ��ʶ
		if (PhotonNetwork.IsConnected && !photonView.IsMine)
		{
			if (selfMinimap != null) selfMinimap.SetActive(false);
			return;
		}
		*/
	}

	private void OnDestroy()
	{
		// ע���¼�
		gameLogicHandler.frozen -= Freeze;
		gameLogicHandler.unfrozen -= Unfreeze;
		gameLogicHandler.died -= Die;
		gameLogicHandler.respawned -= Respawn;
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
		// ����ǿ���ƶ��������ƶ������壩
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
						forcedMoveObject.SetPositionLock(false);    // �������
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
		Vector3 camForward = new Vector3(playerViewCam.transform.forward.x, 0, playerViewCam.transform.forward.z);
		Vector3 camRight = new Vector3(playerViewCam.transform.right.x, 0, playerViewCam.transform.right.z);
		if (curState == Action.Idle || curState == Action.Pushing || curState == Action.FreeRun)
			playerRigidbody.velocity = useAbsoluteDirection ? new Vector3(xV, 0, yV) : (xV * camRight + yV * camForward).normalized * playerMovingSpeed + new Vector3(0, playerRigidbody.velocity.y, 0);

	}

	/// <summary>
	/// Kick the bomb into specific direction
	/// </summary>
	public void Kick()
	{
		if (!CheckAnimatorState("Push Idle", "Push Run")) return;
		if (pushController.GetCarriedType() != PushableObject.CarryType.Bomb) return;
		ChangeState(Action.Kick);
		pushController.Kick(kickSpeed, kickDelay, transform.forward);
	}

	/// <summary>
	/// ��ʼ�ƶ����������ƶ�����
	/// ����Ѿ��������ƶ������壬���������������ӽ������һ���ɱ��ƶ������峢�Կ�ʼ�ƶ�
	/// </summary>
	public void ChangePushState()
	{
		if (CheckAnimatorState("Push Idle", "Push Run") &&
			curState != Action.Kick &&
			curState != Action.FillingCannon)   // ��ֹ��ը����װը��ʱ���ը���������������Ķ���Ч��
		{
			// ���������ƶ�������
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
		SetPushable(true);   // ��������Ա������ƶ�
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
	/// ���ض�λ�ø���
	/// </summary>
	/// <param name="id"></param>
	private void Respawn(string id)
    {
		ChangeState(Action.Idle);
		Transform spawnPoint = SpawnManager.Instance.GetSpawnPoint(gameLogicHandler.playerInfo);
		transform.position = spawnPoint.position;
		transform.rotation = spawnPoint.rotation;
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
		playerAnimator.SetBool("IsFiring", curState == Action.FillingCannon);
		playerAnimator.SetBool("IsFrozen", curState == Action.Frozen);
		playerAnimator.SetBool("IsDead", curState == Action.Reborn);
		//playerAnimator.SetBool("IsForcedMove", curState == Action.ForcedMove);
	}

	/// <summary>
	/// ����һЩ��Ҫ�����¼����жϿ�ʼ�ͽ�����״̬
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
		forcedMoveObject.SetPositionLock(true);   // �ڴ˹����б��ƶ�����������
		Vector3 bombPosition = pushable.transform.position;

		// ������ת����������ը���ķ���
		Vector3 bombDir = bombPosition - transform.position;
		bombDir.y = 0f;
		bombDir.Normalize();
		forcedStartRotation = transform.rotation;
		forcedEndRotation = Quaternion.LookRotation(bombDir, Vector3.up);

		// ������ת֮��İ��˵�λ��
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
	/// �����ǰ��ɫ�����ޣ�������ΪFreeRun
	/// </summary>
	private void ResetStateIfNotConstrained()
	{
		if (!CheckStateConstrainted())
		{
			ChangeState(Action.FreeRun);
		}
	}

	/// <summary>
	/// ȷ�ϵ�ǰ״̬�Ƿ�Ϊ���޵�״̬
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
	/// �жϵ�ǰ�����Ƿ��������״̬����һ��
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
				Debug.Log("Changed to Pushing");
				curState = Action.Pushing;
				break;
			case Action.Frozen:
				StopPushing();
				curState = Action.Frozen;
				break;
			case Action.Kick:
				Debug.Log("Changed to Kick");
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
				Debug.Log("Changed to ForcedMove");
				curState = Action.ForcedMove;
				break;
		}
	}

	public Action GetCurrentState()
	{
		return curState;
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
	FillingCannon = 5,
	Reborn = 6,
	ForcedMove = 7,     // ǿ���ƶ�
}