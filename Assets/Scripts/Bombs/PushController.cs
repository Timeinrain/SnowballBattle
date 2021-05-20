using System.Collections;
using UnityEngine;
using Photon.Pun;
using System;
using System.Collections.Generic;

namespace core.zqc.bombs
{
	/// <summary>
	/// �����ƶ�����߼�
	/// </summary>
	public class PushController : MonoBehaviourPun
	{
		public PlayerController playerController;
		public Transform carryPoint;         // �ƶ�������õ�λ��

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
			// ͬ���ƶ�����λ��
			if (carriedObject != null && !waitForCarrying)
			{
				PhotonView bombView = PhotonView.Get(carriedObject);
				carriedObject.UpdateTransform(carryPoint.position, carryPoint.rotation);
				bombView.RPC("UpdateTransform", RpcTarget.Others, carryPoint.position, carryPoint.rotation);
			}
		}

		[PunRPC]
		public void Kick(float bombShootSpeed, float kickDelay,Vector3 forwardDir)
		{
			if (carriedObject != null && !waitForCarrying)
			{
				Bomb bomb = carriedObject.GetComponent<Bomb>();
                if (bomb != null)
				{
					PhotonView bombView = PhotonView.Get(bomb);
					bomb.DelayShoot(forwardDir * bombShootSpeed, kickDelay);
					bomb.OnDetached();
					bombView.RPC("DelayShoot", RpcTarget.Others, forwardDir * bombShootSpeed, kickDelay);
					bombView.RPC("OnDetached", RpcTarget.Others);
					carriedObject = null;
				}

			}
		}

		public PushableObject GetCarried()
		{
			return carriedObject;
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
		/// ����һ����Χ������Ŀ����ƶ�������
		/// </summary>
		/// <returns></returns>
		public PushableObject GetPushableInRange()
		{
			if (pushableInRange.Count == 0)
				return null;
			else
			{
				// �ҵ���������Ŀ��ƶ�����
				int minDistNum = -1;
				float minDist = 0;
				for (int i = pushableInRange.Count - 1; i >= 0; i--)
				{
					if (pushableInRange[i] == null)
					{
						// ɾ��������
						pushableInRange.RemoveAt(i);
						if (minDistNum != -1) minDistNum--;
						continue;
					}
					if (!pushableInRange[i].CheckPushable())
					{
						// ������ʱ�������ƶ�������
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
		/// ����������������Ƶ�����
		/// </summary>
		public void DetachCurrentPushing()
		{
			if (pushableInRange.Contains(carriedObject))
			{
				pushableInRange.Remove(carriedObject);
			}
			carriedObject = null;
		}

		/// <summary>
		/// ��PushController����ƶ�������
		/// </summary>
		public void AttachPushable(PushableObject pushable)
		{
			carriedObject = pushable;
		}
	}
}