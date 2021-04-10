using UnityEngine;

public class BombControl : MonoBehaviour
{
    public Transform carryPoint;            // ը����Я��ʱ���õ�λ��
    public float bombMoveSpeed = 0.2f;      // Я��ը��ʱ��ը�����ƶ��ٶ�
    public float bombShootSpeed = 2f;       // ը��������ٶ�

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
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 dir = ray.direction;
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
                carriedBomb.GroundMove(dir * bombMoveSpeed);
            }
        }
    }
}
