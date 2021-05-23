using System.Collections;
using UnityEngine;
using Photon.Pun;
using System;
using System.Collections.Generic;
using core.zqc.bombs;

/// <summary>
/// 处理推动相关逻辑
/// </summary>
public class PushController : MonoBehaviourPun
{
	public PlayerController playerController;
	public Transform bombCarryPoint;           // 炸弹放置的位置
	public Transform frozenAllyCarryPoint;     // 被冰冻的队友放置的位置

	public List<PushableObject> pushableInRange = new List<PushableObject>();
	PushableObject carriedObject = null;
	bool waitForCarrying = false;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Bomb") || other.CompareTag("Player"))
		{
			PushableObject pushable = other.gameObject.GetComponent<PushableObject>();
			pushableInRange.Add(pushable);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Bomb") || other.CompareTag("Player"))
		{
			PushableObject pushable = other.gameObject.GetComponent<PushableObject>();
			pushableInRange.Remove(pushable);
		}
	}

	void FixedUpdate()
	{
		// 同步推动物体位置
		if (carriedObject != null && !waitForCarrying)
		{
			if (carriedObject.type == PushableObject.CarryType.Bomb)
			{
				PhotonView bombView = PhotonView.Get(carriedObject);
				carriedObject.UpdateTransform(bombCarryPoint.position, bombCarryPoint.rotation);
				bombView.RPC("UpdateTransform", RpcTarget.Others, bombCarryPoint.position, bombCarryPoint.rotation);
			}
			else if (carriedObject.type == PushableObject.CarryType.Player)
			{
				PhotonView playerView = PhotonView.Get(carriedObject);
				carriedObject.UpdateTransform(frozenAllyCarryPoint.position, frozenAllyCarryPoint.rotation);
				playerView.RPC("UpdateTransform", RpcTarget.Others, frozenAllyCarryPoint.position, frozenAllyCarryPoint.rotation);
			}
		}
	}

	[PunRPC]
	public void Kick(float bombShootSpeed, float kickDelay,Vector3 forwardDir)
	{
		if (carriedObject != null && !waitForCarrying)
		{
			if (carriedObject.type != PushableObject.CarryType.Bomb)
				return;

			Bomb bomb = carriedObject.GetComponent<Bomb>();
            if (bomb != null)
			{
				PhotonView bombView = PhotonView.Get(bomb);
				bomb.DelayShoot(forwardDir * bombShootSpeed, kickDelay);
				bomb.Detach();
				bombView.RPC("DelayShoot", RpcTarget.Others, forwardDir * bombShootSpeed, kickDelay);
				bombView.RPC("Detach", RpcTarget.Others);
				carriedObject = null;
			}
		}
	}

	public PushableObject GetCarried()
	{
		return carriedObject;
	}

	/// <summary>
	/// 返回一个范围内最近的可以推动的物体
	/// </summary>
	/// <returns></returns>
	public PushableObject GetPushableInRange()
	{
		if (pushableInRange.Count == 0)
			return null;
		else
		{
			// 找到距离最近的可推动物体
			int minDistNum = -1;
			float minDist = 0;
			for (int i = pushableInRange.Count - 1; i >= 0; i--)
			{
				if (pushableInRange[i] == null)
				{
					// 删除空物体
					pushableInRange.RemoveAt(i);
					if (minDistNum != -1) minDistNum--;
					continue;
				}
				if (!pushableInRange[i].CheckPushable())
				{
					// 跳过暂时不可以推动的物体
					continue;
				}
				float dist = (pushableInRange[i].transform.position - transform.position).sqrMagnitude;
				if (minDistNum == -1)
				{
					minDistNum = i;
					minDist = (pushableInRange[i].transform.position - transform.position).sqrMagnitude;
				}
				else if (dist < minDist)
				{
					minDistNum = i;
					minDist = dist;
				}
			}
			if (minDistNum == -1) return null;
			return pushableInRange[minDistNum];
		}
	}

	/// <summary>
	/// 主动解除现在正在推的物体
	/// </summary>
	public void DetachCurrentPushing()
	{
		carriedObject = null;
	}

	public void StopPlayerPushing()
    {
		playerController.StopPushing();
    }

	/// <summary>
	/// 让PushController获得推动的物体
	/// </summary>
	public void AttachPushable(PushableObject pushable)
	{
		carriedObject = pushable;
		pushable.Attach(playerController);
	}

	public PushableObject.CarryType GetCarriedType()
    {
		if (carriedObject == null) return PushableObject.CarryType.Null;
		return carriedObject.type;
    }
}