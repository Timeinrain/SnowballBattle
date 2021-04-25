using System.Collections;
using UnityEngine;

using core.zqc.players;

namespace core.zqc.bombs
{
    public class BombControl : MonoBehaviour
    {
        public PlayerAnimator playerAnimator;
        public PlayerController playerController;  // 角色

        public float bombShootSpeed = 50f;         // 炸弹发射的力

        [Header("角色拿到炸弹的动画设定")]
        public Transform carryPoint;         // 炸弹放置的位置
        public float preWaitTime = 0.1f;     // 等待步行动画结束的时间
        public float attachTime = 0.1f;      // 炸弹吸附到carryPoint上的时间
        public float rotateTime = 0.2f;      // 为拿到炸弹旋转人物的动画时间

        Bomb carriedBomb = null;
        bool waitForCarrying = false; 

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Bomb"))
            {
                if (!waitForCarrying && carriedBomb == null)
                {
                    carriedBomb = other.transform.GetComponent<Bomb>();
                    waitForCarrying = true;
                    StartCoroutine(PlayGetBombAnimation(carriedBomb));
                }
            }
        }

        void Update()
        {
            // 按下左键将冰壶炸弹发射出去
            if (Input.GetMouseButton(0) && carriedBomb != null && !waitForCarrying)
            {
                Vector3 dir = transform.forward;

                carriedBomb.Shoot(dir * bombShootSpeed);
                carriedBomb.OnDetached();
                carriedBomb = null;
            }
        }

        void FixedUpdate()
        {
            if (carriedBomb != null && !waitForCarrying)
            {
                carriedBomb.UpdateTransform(carryPoint.position, carryPoint.rotation);
            }
        }

        /// <summary>
        /// 角色拿到炸弹的动画
        /// </summary>
        /// <param name="bomb"></param>
        /// <returns></returns>
        IEnumerator PlayGetBombAnimation(Bomb bomb)
        {
            playerController.SetAnimationFlag(true);

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
                playerController.transform.rotation = Quaternion.Slerp(startRotation, endRotation, timer / rotateTime);
                yield return new WaitForFixedUpdate();
            }
            playerController.transform.rotation = endRotation;

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
            playerController.SetAnimationFlag(false);
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