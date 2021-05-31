using Photon.Pun;
using UnityEngine;
/// <summary>
/// ��ҿ��ƽű����������ڲ�����
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
	public float slopeLimit;    // ������½�

	[Range(1, 50)]
	public float viewFieldRadiance;

	[Header("Push Control Settings")]
	public PushController pushController;
	[Range(1, 100)]
	public float kickSpeed;              // ��ը�����ٶ�
	[Range(0.0f, 1.0f)]
	public float rotateTime;             // ��ը��ǰת��ʱ��
	[Range(0.0f, 2.0f)]
	public float kickDelay;              // �߶�����ʼ��ʵ���߳�ը�����ӳ�

	[Header("Throw Settings")]
	[Range(0, 0.5f)]
	public float minThrowSpeedY;         // ��ֱ�������СͶ���ٶ�Ȩ��
	[Range(0, 0.5f)]
	public float maxThrowSpeedY;         // ��ֱ��������Ͷ���ٶ�Ȩ��

	private float maxThrowDistance;      // ����õ������ˮƽͶ������

	[Header("Other Settings")]
	[Range(0, 30)]
	public float impulseY;               // ��ը��ը��֮��ĳ������Y������
	[Range(0, 30)]
	public float impulseXZ;              // ��ը��ը��֮��ĳ��������ֱY��ƽ������
	public GameObject ice;               // ��ɫ������ʱ����

	private Action curState;

	private bool useThrow;               // ���ú�Ͷ��������ߵĶ���

	#region ǿ���ƶ����
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

		// Ϊ��Ϸ�߼���������¼�
		gameLogicHandler = GetComponent<Character>();
		gameLogicHandler.frozen += Freeze;
		gameLogicHandler.unfrozen += Unfreeze;
		gameLogicHandler.died += Die;
		gameLogicHandler.respawned += Respawn;
		gameLogicHandler.affectedByExplosion += OnAffectedByExplosion;

		/*
		//������Ǳ��ˣ������ض�Ӧ����һ��С��ͼ��ʶ
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
					//ѩ��
					useThrow = false;
					break;
				}
			case 2:
				{
					//��ʥ��
					useThrow = true;
					break;
				}
			case 3:
				{
					//�ǹ��Ǳ�
					useThrow = true;
					break;
				}
		}

		float t = maxThrowSpeedY * kickSpeed / Physics.gravity.magnitude * 2f;
		maxThrowDistance = t * (1f - maxThrowSpeedY) * kickSpeed;
	}

    private void OnDestroy()
	{
		// ע���¼�
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
		// �յ���ը���ʱ�����ƶ�
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

			// ��ȡ�Ӵ����淨������
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
				// ���ݵ����Զ��ȷ������
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
	/// ��ʼ�ƶ����������ƶ�����
	/// ����Ѿ��������ƶ������壬���������������ӽ������һ���ɱ��ƶ������峢�Կ�ʼ�ƶ�
	/// </summary>
	public void ChangePushState()
	{		
		if (CheckAnimatorState("Push Idle", "Push Run") &&
			pushController.GetCarriedType() == CarryType.Player)   // ֻ���ƶ����Ǳ����Ķ���ʱ���ܷ���
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
		playerRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
	} 


	/// <summary>
	/// ����ը����
	/// </summary>
	/// <param name="sourcePos">ը����Դ</param>
	/// <param name="isFrozenWhenExplosion">�����Ƿ񱻱���</param>
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
	/// �������λ�û��һ����ά����
	/// </summary>
	/// <param name="position">���ص�����</param>
	/// <returns>�����λ���Ƿ�Ϸ�</returns>
	private bool LocateMousePosition(out Vector3 position)
    {
		// ������ʾ�����Ļ�����С
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
	Kick = 4,           // �����ߺ�Ͷ��
	FillingCannon = 5,
	Reborn = 6,
	ForcedMove = 7,     // ǿ���ƶ�
}