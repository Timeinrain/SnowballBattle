using UnityEngine;

namespace core.zqc.bombs
{
    public class BombControl : MonoBehaviour
    {
        public Transform carryPoint;               // ը����Я��ʱ���õ�λ��
        public float bombMoveAcceleration = 100f;  // Я��ը��ʱ��ը�����ƶ����ٶ�
        public float bombShootSpeed = 80f;         // ը���������

        Bomb carriedBomb = null;
        bool waitForCarrying = false;

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Bomb"))
            {
                if (!waitForCarrying && carriedBomb == null)
                {
                    Debug.Log("Get bomb");
                    // ��������ģ�����������ϣ�����Ҫ��ȡ���������ģ���ڸ���������
                    carriedBomb = other.transform.GetComponent<Bomb>();
                    carriedBomb.OnAttached();
                }
            }
        }

        void Update()
        {
            // �������������ը�������ȥ
            if (Input.GetMouseButton(0) && carriedBomb != null)
            {
                Vector3 dir = transform.forward;
                dir.y = 0f;  // ���������˶�

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
                    // ��ը���ƶ�����Խ�ɫ�Ĺ̶�λ��carryPoint
                    Vector3 dir = distance.normalized;
                    carriedBomb.GroundMove(dir * bombMoveAcceleration);
                }
            }
        }
    }
}