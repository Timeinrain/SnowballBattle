using System.Collections;
using UnityEngine;
using Photon.Pun;
using System;
using System.Collections.Generic;

namespace core.zqc.bombs
{
	/// <summary>
	/// 处理炸弹相关逻辑
	/// </summary>
	public class BombController : MonoBehaviourPun
	{
		public PlayerController playerController;
		public Transform carryPoint;         // 炸弹放置的位置

		List<Bomb> bombsInRange = new List<Bomb>();
		Bomb carriedBomb = null;
		bool waitForCarrying = false;

		private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("Bomb"))
			{
				Bomb bomb = other.gameObject.GetComponent<Bomb>();
				bombsInRange.Add(bomb);
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (other.CompareTag("Bomb"))
			{
				Bomb bomb = other.gameObject.GetComponent<Bomb>();
				bombsInRange.Remove(bomb);
			}
		}

		void FixedUpdate()
		{
			// 同步炸弹位置
			if (carriedBomb != null && !waitForCarrying)
			{
				PhotonView bombView = PhotonView.Get(carriedBomb);
				carriedBomb.UpdateTransform(carryPoint.position, carryPoint.rotation);
				bombView.RPC("UpdateTransform", RpcTarget.Others, carryPoint.position, carryPoint.rotation);
			}
		}

		[PunRPC]
		public void Kick(float bombShootSpeed, float kickDelay,Vector3 forwardDir)
		{
			if (carriedBomb != null && !waitForCarrying)
			{
				PhotonView bombView = PhotonView.Get(carriedBomb);
				carriedBomb.DelayShoot(forwardDir * bombShootSpeed, kickDelay);
				carriedBomb.OnDetached();
				bombView.RPC("DelayShoot", RpcTarget.Others, forwardDir * bombShootSpeed, kickDelay);
				bombView.RPC("OnDetached", RpcTarget.Others);
				carriedBomb = null;
			}
		}

		public Bomb GetCarriedBomb()
		{
			return carriedBomb;
		}


		/*
        /// <summary>
        /// 角色拿到炸弹的动画
        /// </summary>
        /// <param name="bomb"></param>
        /// <returns></returns>
        IEnumerator PlayGetBombAnimation(Bomb bomb)
        {
            playerAnimator.SetAnimationFlag(true);

            yield return new WaitForSeconds(preWaitTime);

            // 人物旋转到可以拿起炸弹的方位
            if (bomb == null)
            {
                ResetPlayerState();
                yield break;
            }
            Vector3 bombDir = bomb.transform.position - transform.position;
            bombDir.y = 0f;
            bombDir.Normalize();
            Quaternion startRotation = transform.rotation;
            Quaternion endRotation = Quaternion.LookRotation(bombDir, Vector3.up);

            for (float timer = 0f; timer <= rotateTime; timer += Time.fixedDeltaTime)
            {
                playerAnimator.SetRotation(Quaternion.Slerp(startRotation, endRotation, timer / rotateTime));
                yield return new WaitForFixedUpdate();
            }
            playerAnimator.SetRotation(endRotation);

            if (bomb == null)
            {
                ResetPlayerState();
                yield break;
            }
            // 炸弹吸附到carryPoint的位置
            Vector3 startPosition = bomb.transform.position;
            Vector3 endPosition = carryPoint.position;
            endPosition.y = bomb.transform.position.y; // 不改变炸弹的y坐标
            carryPoint.rotation = bomb.transform.rotation; // 初始使旋转和炸弹旋转一致，便于之后计算旋转
            for(float timer = 0f; timer <= attachTime; timer += Time.fixedDeltaTime)
            {
                bomb.transform.position = Vector3.Slerp(startPosition, endPosition, timer / attachTime);
                yield return new WaitForFixedUpdate();
                if (bomb == null)
                {
                    ResetPlayerState();
                    yield break;
                }
            }
            bomb.transform.position = endPosition;

            bomb.OnAttached(this);
            ResetPlayerState();
        }
        */

		/// <summary>
		/// 返回一个范围内最近的炸弹
		/// </summary>
		/// <returns></returns>
		public Bomb GetBombInRange()
		{
			if (bombsInRange.Count == 0)
				return null;
			else
			{
				// 找到距离最近的炸弹
				int minDistNum = -1;
				float minDist = 0;
				for (int i = bombsInRange.Count - 1; i >= 0; i--)
				{
					if (bombsInRange[i] == null)
					{
						bombsInRange.RemoveAt(i);
						if (minDistNum != -1) minDistNum--;
						continue;
					}
					float dist = (bombsInRange[i].transform.position - transform.position).sqrMagnitude;
					if (minDistNum == -1)
					{
						minDistNum = i;
						minDist = (bombsInRange[i].transform.position - transform.position).sqrMagnitude;
					}
					else if (dist < minDist)
					{
						minDistNum = i;
						minDist = dist;
					}
				}
				if (minDistNum == -1) return null;
				return bombsInRange[minDistNum];
			}
		}

		/// <summary>
		/// 主动解除炸弹
		/// </summary>
		public void DetachCurrentBomb()
		{
			if (bombsInRange.Contains(carriedBomb))
			{
				bombsInRange.Remove(carriedBomb);
			}
			carriedBomb = null;
		}

		/// <summary>
		/// 让BombController获得炸弹
		/// </summary>
		public void AttachBomb(Bomb bomb)
		{
			carriedBomb = bomb;
		}
	}
}