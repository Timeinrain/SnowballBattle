using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �����ɫ��Ϸ�߼�
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class Character : MonoBehaviour
{
    /// <summary>
    /// ���Ա�ѩ�����/ը��ը���Ĵ���������ֵ���㽫���½�ɫ������
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
