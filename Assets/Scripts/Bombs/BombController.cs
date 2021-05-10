using System.Collections;
using UnityEngine;
using Photon.Pun;
using System;
using System.Collections.Generic;

namespace core.zqc.bombs
{
	/// <summary>
	/// ����ը������߼�
	/// </summary>
	public class BombController : MonoBehaviourPun
	{
		public PlayerController playerController;
		public Transform carryPoint;         // ը�����õ�λ��

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
			// ͬ��ը��λ��
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
        /// ��ɫ�õ�ը���Ķ���
        /// </summary>
        /// <param name="bomb"></param>
        /// <returns></returns>
        IEnumerator PlayGetBombAnimation(Bomb bomb)
        {
            playerAnimator.SetAnimationFlag(true);

            yield return new WaitForSeconds(preWaitTime);

            // ������ת����������ը���ķ�λ
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
            // ը��������carryPoint��λ��
            Vector3 startPosition = bomb.transform.position;
            Vector3 endPosition = carryPoint.position;
            endPosition.y = bomb.transform.position.y; // ���ı�ը����y����
            carryPoint.rotation = bomb.transform.rotation; // ��ʼʹ��ת��ը����תһ�£�����֮�������ת
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
		/// ����һ����Χ�������ը��
		/// </summary>
		/// <returns></returns>
		public Bomb GetBombInRange()
		{
			if (bombsInRange.Count == 0)
				return null;
			else
			{
				// �ҵ����������ը��
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
		/// �������ը��
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
		/// ��BombController���ը��
		/// </summary>
		public void AttachBomb(Bomb bomb)
		{
			carriedBomb = bomb;
		}
	}
}