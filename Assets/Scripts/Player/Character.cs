using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// 处理角色游戏逻辑
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class Character : MonoBehaviourPun
{
    /// <summary>
    /// 可以被雪球击到/炸弹炸到的次数，生命值归零将导致角色被冰冻
    /// </summary>
    public int maxHealth = 2;


    public string id;

    /// <summary>
    /// 解冻所需时间
    /// </summary>
    public float unfreezeTime = 5f;

    /// <summary>
    /// 可以被订阅的事件
    /// </summary>
    public event System.Action<string> frozen, unfrozen, died;
    public event System.Action<string, int> healed, damaged;

    private Team team;

    private bool isFrozen = false;
    private bool isUnfreezing = false;
    private float unfreezeTimer = 0f;

    public int Health
    {
        get; private set;
    }

    private void Start()
    {
        Health = maxHealth;
    }

    private void Update()
    {
        HandleUnfreezeProcess();
    }

    private void SafelyDoAction(System.Action<string> action, string id)
    {
        if (action != null)
        {
            action(id);
        }
    }
    private void SafelyDoAction(System.Action<string, int> action, string id, int val)
    {
        if (action != null)
        {
            action(id, val);
        }
    }

    public void DealDamage(int damage = 1)
    {
        Health -= damage;
        SafelyDoAction(damaged, id, damage);
        if (Health <= 0)
        {
            Freeze();
        }
    }

    private void Freeze()
    {
        isFrozen = true;
        SafelyDoAction(frozen, id);
    }

    private void Unfreeze()
    {
        isFrozen = false;
        SafelyDoAction(unfrozen, id);
        Heal(1);     // 确保有足够的血量活下来
    }

    public void StartUnfreezeCountdown()
    {
        isUnfreezing = true;
        unfreezeTimer = 0f;
    }

    public void StopUnfreezeCountdown()
    {
        isUnfreezing = false;
        unfreezeTimer = 0f;
    }

    /// <summary>
    /// 处理解冻流程
    /// </summary>
    private void HandleUnfreezeProcess()
    {
        if (!isFrozen || !isUnfreezing)
            return;

        unfreezeTimer += Time.deltaTime;
        if (unfreezeTimer > unfreezeTime)
        {
            Unfreeze();
        }
    }

    public void Heal(int heal = 1)
    {
        Health += heal;
        SafelyDoAction(healed, id, heal);
    }

    public void SetTeam(Team team)
    {
        this.team = team;
    }

    public Team GetTeam()
    {
        return team;
    }
}
