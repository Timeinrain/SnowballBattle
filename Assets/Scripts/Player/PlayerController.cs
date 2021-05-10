using core.zqc.bombs;
using Photon.Pun;
using UnityEditor;
using UnityEngine;
/// <summary>
/// ��ҿ��ƽű����������ڲ�����
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
	public float kickSpeed;              // ��ը�����ٶ�
	[Range(0.0f, 2.0f)]
	public float kickDelay;              // �߶�����ʼ��ʵ���߳�ը�����ӳ�

	[Header("Other Animation Settings")]
	[Range(0.0f, 10.0f)]
	public float fireDelay;              // ��𶯻���ʼ��ʵ�ʽ�ը���ͳ����ӳ�

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

    #region ǿ���ƶ����
	private Vector3 forcedMoveDir;
	private Quaternion forcedStartRotation;
	private Quaternion forcedEndRotation;
	private float totolTime;
	private float forcedMoveTimer = 0f;
	private Bomb carriedBomb;
    #endregion

    void Awake()
	{
		//������Ǳ��ˣ������ض�Ӧ����һ��С��ͼ��ʶ
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
        // ����ǿ���ƶ�
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
			bomb.StopExplosionCountdown();    // ը��������������̨�����б�ը
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
	/// ����һЩ��Ҫ�����¼����жϿ�ʼ�ͽ�����״̬
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

		// ������ת����������ը���ķ���
		Vector3 bombDir = bombPosition - transform.position;
		bombDir.y = 0f;
		bombDir.Normalize();
		forcedStartRotation = transform.rotation;
		forcedEndRotation = Quaternion.LookRotation(bombDir, Vector3.up);

		// ������ת֮��İ��˵�λ��
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
	/// �����ǰ��ɫ�����ޣ�������ΪFreeRun
	/// </summary>
	private void ResetStateIfNotConstrained()
    {
		if(!CheckStateConstrainted())
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
	/// ȷ�ϵ�ǰ״̬�Ƿ�Ϊ����״̬����Щ״̬�ڶ�������֮����Զ��л�
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
	/// �жϵ�ǰ�����Ƿ��������״̬����һ��
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
	ForcedMove = 7,     // ǿ���ƶ�
}