using UnityEngine;

namespace core.zqc.bombs
{
    public class BombControl : MonoBehaviour
    {
        public Transform carryPoint;               // 炸弹被携带时放置的位置
        public float bombMoveAcceleration = 100f;  // 携带炸弹时，炸弹的移动加速度
        public float bombShootSpeed = 80f;         // 炸弹发射的力

        Bomb carriedBomb = null;
        bool waitForCarrying = false;

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Bomb"))
            {
                if (!waitForCarrying && carriedBomb == null)
                {
                    Debug.Log("Get bomb");
                    // 这里由于模型在子物体上，所以要获取父对象，如果模型在父对象则不用
                    carriedBomb = other.transform.GetComponent<Bomb>();
                    carriedBomb.OnAttached();
                }
            }
        }

        void Update()
        {
            // 按下左键将冰壶炸弹发射出去
            if (Input.GetMouseButton(0) && carriedBomb != null)
            {
                Vector3 dir = transform.forward;
                dir.y = 0f;  // 冰壶贴地运动

                carriedBomb.Shoot(dir * bombShootSpeed);
                carriedBomb.OnDetached();
                carriedBomb = null;
            }
        }

        void FixedUpdate()
        {
            if (carriedBomb != null)
            {
                Vector3 distance = carryPoint.position - carriedBomb.transform.position;
                if (distance.sqrMagnitude > 1f)
                {
                    // 将炸弹移动到相对角色的固定位置carryPoint
                    Vector3 dir = distance.normalized;
                    carriedBomb.GroundMove(dir * bombMoveAcceleration);
                }
            }
        }
    }
}