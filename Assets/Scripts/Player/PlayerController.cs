using core.zqc.bombs;
using Photon.Pun;
using UnityEngine;
/// <summary>
/// 玩家控制脚本，联机仍在测试中
/// </summary>
public class PlayerController : PushableObject
{

    Rigidbody playerRigidbody;
    Animator playerAnimator;
    Player playerInfoInstance;
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
    public float kickSpeed;              // 踢炸弹初速度
    [Range(0.0f, 2.0f)]
    public float kickDelay;              // 踢动画开始到实际踢出炸弹的延迟

    [Header("Other Animation Settings")]
    [Range(0.0f, 10.0f)]
    public float fireDelay;              // 角色点火动画持续时间

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
    // private Cannon nearbyCannon;

    #region 强制移动相关
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
        type = CarryType.Player;

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

        // 为游戏逻辑处理添加事件
        gameLogicHandler = GetComponent<Character>();
        gameLogicHandler.frozen += Freeze;
        gameLogicHandler.unfrozen += Unfreeze;

        ChangeState(Action.FreeRun);
        SetPushable(false);
    }

    private void OnDestroy()
    {
        // 注销事件
        gameLogicHandler.frozen -= Freeze;
        gameLogicHandler.unfrozen -= Unfreeze;
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
        if (curState == Action.Idle || curState == Action.Pushing || curState == Action.FreeRun)
            playerRigidbody.velocity = new Vector3(xV, 0, yV).normalized * playerMovingSpeed + new Vector3(0, playerRigidbody.velocity.y, 0);
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
    /// 开始推动物体或结束推动物体
    /// 如果已经有正在推动的物体，则会放下它，否则会接近最近的一个可被推动的物体尝试开始推动
    /// </summary>
    public void ChangePushState()
    {
        if (CheckAnimatorState("Push Idle", "Push Run") &&
            curState != Action.Kick &&
            curState != Action.FillingCannon)   // 防止踢炸弹和装炸弹时解除炸弹，发生不期望的动画效果
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

    /*
    /// <summary>
    /// Try to fill a cannon if there is a cannon nearby
    /// </summary>
    public void FillCannon()
    {
        if (!CheckAnimatorState("Push Idle", "Push Run")) return;
        if (nearbyCannon != null && pushController.GetCarriedType() == CarryType.Bomb)
        {
            PushableObject carried = pushController.GetCarried();
            if (carried != null)
            {
                Bomb bomb = carried.GetComponent<Bomb>();
                if (bomb != null)
                {
                    ChangeState(Action.FillingCannon);
                    nearbyCannon.FillBomb(bomb, fireDelay);
                }
            }
        }
    }
    */

    public void Freeze(string id)
    {
        ChangeState(Action.Frozen);
        SetPushable(true);   // 冰冻后可以被队友推动
    }

    public void Unfreeze(string id)
    {
        ChangeState(Action.Idle);
        SetPushable(false);
        StopCarrierPushing();
        playerRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
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
        //playerAnimator.SetBool("IsForcedMove", curState == Action.ForcedMove);
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

    /*
    public void AddNearbyCannon(Cannon cannon)
    {
        nearbyCannon = cannon;
    }

    public void RemoveNearbyCannon()
    {
        nearbyCannon = null;
    }
    */
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
    ForcedMove = 7,     // 强制移动
}