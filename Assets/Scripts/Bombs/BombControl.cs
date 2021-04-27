using System.Collections;
using UnityEngine;

using core.zqc.players;

namespace core.zqc.bombs
{
    public class BombControl : MonoBehaviour
    {
        public PlayerAnimator playerAnimator;

        public float bombShootSpeed = 50f;         // ը���������

        [Header("��ɫ�õ�ը���Ķ����趨")]
        public Transform carryPoint;         // ը�����õ�λ��
        public float preWaitTime = 0.1f;     // �ȴ����ж���������ʱ��
        public float attachTime = 0.1f;      // ը��������carryPoint�ϵ�ʱ��
        public float rotateTime = 0.2f;      // Ϊ�õ�ը����ת����Ķ���ʱ��

        [Header("��ɫ����ը���Ķ����趨")]
        public float kickDelay;              // ��ҷ�������ָ�ʵ���򱻷�����ӳ�ʱ��

        Bomb carriedBomb = null;
        bool waitForCarrying = false; 

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Bomb"))
            {
                if (!waitForCarrying && carriedBomb == null)
                {
                    OnGetBomb(other.transform.GetComponent<Bomb>());
                }
            }
        }

        void Update()
        {
            // �������������ը�������ȥ
            if (Input.GetMouseButton(0) && carriedBomb != null && !waitForCarrying)
            {
                KickBomb();
            }
        }

        void FixedUpdate()
        {
            if (carriedBomb != null && !waitForCarrying)
            {
                carriedBomb.UpdateTransform(carryPoint.position, carryPoint.rotation);
            }
        }

        void OnGetBomb(Bomb bomb)
        {
            carriedBomb = bomb;
            waitForCarrying = true;
            playerAnimator.SetPushing();
            StartCoroutine(PlayGetBombAnimation(carriedBomb));
        }

        void KickBomb()
        {
            playerAnimator.SetKick();

            Vector3 dir = transform.forward;
            carriedBomb.DelayShoot(dir * bombShootSpeed, kickDelay);
            carriedBomb.OnDetached();
            carriedBomb = null;
        }

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

        void ResetPlayerState()
        {
            waitForCarrying = false;
            playerAnimator.SetAnimationFlag(false);
        }

        /// <summary>
        /// �������ը��
        /// </summary>
        public void DetachCurrentBomb()
        {
            carriedBomb = null;
        }
    }
}