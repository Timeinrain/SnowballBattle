using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 处理角色游戏逻辑
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class Character : MonoBehaviour
{
    /// <summary>
    /// 可以被雪球击到/炸弹炸到的次数，生命值归零将导致角色被冰冻
    /// </summary>
    public int maxHealth = 2;

    private Team team;
    private PlayerController playerController;
    public int Health
    {
        get; private set;
    }

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        Health = maxHealth;
    }

    private void Freeze()
    {
        playerController.Freeze();
    }

    public void DealDamage(int damage = 1)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Freeze();
        }
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
