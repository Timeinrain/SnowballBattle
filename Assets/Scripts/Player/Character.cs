using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Sirenix.OdinInspector;
using UnityEngine.UI;

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

	[ShowInInspector]
	public Player playerInfo;

	[Tooltip("解冻所需时间")]
	public float unfreezeTime = 5f;
	[Tooltip("冰冻后可以存活的时间(这个倒计时在解冻时会被暂停)")]
	public float surviveTimeAfterFrozen = 15f;
	[Tooltip("复活时间")]
	public float respawnTime = 20f;

	/// <summary>
	/// 可以被订阅的事件
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
	/// 暂时性的地图枚举
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

	public void TakeDamage(int damage = 1)
	{
		if (Health <= 0) return;

		Health -= damage;
		damaged?.Invoke(id, damage);
		if (Health <= 0)
		{
			if (mapType == MapType.Snow)
				Freeze();
            else
				Die();
		}
	}

	public void Heal(int heal = 1)
	{
		Health += heal;
		if (Health > maxHealth) Health = maxHealth;
		healed?.Invoke(id, heal);
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

	private void Unfreeze()
	{
		isFrozen = false;
		surviveTimer = 0f;
		unfrozen?.Invoke(id);
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
	/// 处理冰冻后存活时间
	/// </summary>
	private void HandleFrozenSurviveTime()
    {
		if (isFrozen && !isUnfreezing)
		{
			surviveTimer += Time.deltaTime;
			if (surviveTimer > surviveTimeAfterFrozen)
			{
				// 冰冻时间过长，角色死亡
				isFrozen = false;
				Die();
			}
		}
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

    #endregion

	private void Die()
    {
		isDead = true;
		respawnTimer = 0f;
		died?.Invoke(id);
	}

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
	/// 每帧处理复活的时间
	/// </summary>
	private void HandleRespawnProcess()
    {
		if (!isDead)
			return;

		respawnTimer += Time.deltaTime;
		if (respawnTimer > respawnTime)
		{
			Respawn();
		}
	}

}
