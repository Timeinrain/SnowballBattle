using System.Collections;
using UnityEngine;

using core.zqc.players;

namespace core.zqc.bombs
{
    public class BombControl : MonoBehaviour
    {
        public PlayerAnimator playerAnimator;

        public float bombShootSpeed = 50f;         // 炸弹发射的力

        [Header("角色拿到炸弹的动画设定")]
        public Transform carryPoint;         // 炸弹放置的位置
        public float preWaitTime = 0.1f;     // 等待步行动画结束的时间
        public float attachTime = 0.1f;      // 炸弹吸附到carryPoint上的时间
        public float rotateTime = 0.2f;      // 为拿到炸弹旋转人物的动画时间

        [Header("角色发射炸弹的动画设定")]
        public float kickDelay;              // 玩家发出发射指令到实际球被发射的延迟时间

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
            // 按下左键将冰壶炸弹发射出去
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

        void ResetPlayerState()
        {
            waitForCarrying = false;
            playerAnimator.SetAnimationFlag(false);
        }

        /// <summary>
        /// 主动解除炸弹
        /// </summary>
        public void DetachCurrentBomb()
        {
            carriedBomb = null;
        }
    }
}