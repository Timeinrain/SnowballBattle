using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Sirenix.OdinInspector;
using UnityEngine.UI;

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

	[ShowInInspector]
	public Player playerInfo;

	[Tooltip("�ⶳ����ʱ��")]
	public float unfreezeTime = 5f;
	[Tooltip("��������Դ���ʱ��(�������ʱ�ڽⶳʱ�ᱻ��ͣ)")]
	public float surviveTimeAfterFrozen = 15f;
	[Tooltip("����ʱ��")]
	public float respawnTime = 20f;

	/// <summary>
	/// ���Ա����ĵ��¼�
	/// </summary>
	public event System.Action<string> frozen, unfrozen, died, respawned;
	public event System.Action<string, int> healed, damaged;

	private Team team;

	private bool isDead = false;
	private bool isFrozen = false;
	private bool isUnfreezing = false;
	private float unfreezeTimer = 0f;
	private float surviveTimer = 0f;
	private float respawnTimer = 0f;

	/// <summary>
	/// ��ʱ�Եĵ�ͼö��
	/// </summary>
	private enum MapType
    {
		Snow,
		Others
    }
	private MapType mapType = MapType.Snow;

	public int Health
	{
		get; private set;
	}

	private void Start()
	{
		Health = maxHealth;
		team = playerInfo.team;
	}

	private void Update()
	{
		HandleRespawnProcess();
		HandleFrozenSurviveTime();
		HandleUnfreezeProcess();
	}

	[PunRPC]
	public void TakeDamage(int damage = 1)
	{
		if (Health <= 0) return;

		Health -= damage;
		damaged?.Invoke(id, damage);
		if (Health <= 0)
		{
			if (mapType == MapType.Snow)
            {
				Freeze();
			}
            else
            {
				Die();
			}
		}
	}

	public Team GetTeam()
	{
		return team;
	}

	#region Scene Snow Mechanics

	private void Freeze()
	{
		isFrozen = true;
		frozen?.Invoke(id);
	}

	[PunRPC]
	private void Unfreeze()
	{
		isFrozen = false;
		surviveTimer = 0f;
		unfrozen?.Invoke(id);
		Health += 1; // ȷ�����㹻��Ѫ��������
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
	/// �����������ʱ��
	/// </summary>
	private void HandleFrozenSurviveTime()
    {
		if (isFrozen && !isUnfreezing)
		{
			surviveTimer += Time.deltaTime;
			if (surviveTimer > surviveTimeAfterFrozen)
			{
				// ����ʱ���������ɫ����
				isFrozen = false;

				if (!PhotonNetwork.LocalPlayer.IsMasterClient)  // �������������ɫ��Ϸ�߼���ֻ���������㲥
					return;
				photonView.RPC("Die", RpcTarget.All);
			}
		}
	}

	/// <summary>
	/// ����ⶳ����
	/// </summary>
	private void HandleUnfreezeProcess()
	{
		if (!PhotonNetwork.LocalPlayer.IsMasterClient)  // �������������ɫ��Ϸ�߼���ֻ���������㲥
			return;

		if (!isFrozen || !isUnfreezing)
			return;

		unfreezeTimer += Time.deltaTime;
		if (unfreezeTimer > unfreezeTime)
		{
			photonView.RPC("Unfreeze", RpcTarget.All);
		}
	}

    #endregion

	[PunRPC]
	private void Die()
    {
		isDead = true;
		respawnTimer = 0f;
		died?.Invoke(id);
	}

	[PunRPC]
	private void Respawn()
    {
		isDead = false;
		isFrozen = false;
		isUnfreezing = false;
		Health = maxHealth;
		unfreezeTimer = 0f;
		surviveTimer = 0f;
		respawnTimer = 0f;
		respawned?.Invoke(id);
	}

	/// <summary>
	/// ÿ֡�������ʱ��
	/// </summary>
	private void HandleRespawnProcess()
    {
		if (!PhotonNetwork.LocalPlayer.IsMasterClient)  // �������������ɫ��Ϸ�߼���ֻ���������㲥
			return;

		if (!isDead)
			return;

		respawnTimer += Time.deltaTime;
		if (respawnTimer > respawnTime)
		{
			photonView.RPC("Respawn", RpcTarget.All);
		}
	}

	/// <summary>
	/// ���ر�����ʣ�����ʱ������
	/// </summary>
	/// <returns></returns>
	public float GetFrozenCountdown()
    {
		return surviveTimeAfterFrozen - surviveTimer;
    }
}
