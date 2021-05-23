using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// �����ɫ��Ϸ�߼�
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class Character : MonoBehaviourPun
{
    /// <summary>
    /// ���Ա�ѩ�����/ը��ը���Ĵ���������ֵ���㽫���½�ɫ������
    /// </summary>
    public int maxHealth = 2;

    public string id;

    /// <summary>
    /// �ⶳ����ʱ��
    /// </summary>
    public float unfreezeTime = 5f;

    /// <summary>
    /// ���Ա����ĵ��¼�
    /// ��һ���������û���id
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
        team = GetComponent<Player>().team;
    }

    private void Update()
    {
        HandleUnfreezeProcess();
    }

    private void SafelyDoAction(System.Action<string> action, string selfID)
    {
        if (action != null)
        {
            action(selfID);
        }
    }
    private void SafelyDoAction(System.Action<string, int> action, string selfID, int val)
    {
        if (action != null)
        {
            action(selfID, val);
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
        Heal(1);     // ȷ�����㹻��Ѫ��������
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
    /// ����ⶳ����
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

    public Team GetTeam()
    {
        return team;
    }
}
