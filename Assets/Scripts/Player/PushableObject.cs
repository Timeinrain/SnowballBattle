using core.zqc.bombs;
using Photon.Pun;
using UnityEngine;

public class PushableObject : MonoBehaviour
{
    public GameObject snowPathFX;

    Rigidbody bombRigidbody;
    BombController carrier = null;

    private void Awake()
    {
        bombRigidbody = GetComponent<Rigidbody>();
    }

    private void OnDestroy()
    {
        if (carrier != null)
            carrier.DetachCurrentBomb();
    }

    /// <summary>
    /// �ڼ���photon view ʱ��Ҫ���͵�message
    /// ������Ҫͬ������Ϣ���ڼ�д��
    /// ������
    /// 1.ը����λ����Ϣ
    /// 2.ը���Ĺ�����Ϣ
    /// 3.ը���ı�ը��Ϣ
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="info"></param>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //todo : 3 infos
            stream.SendNext(bombRigidbody.velocity);
            stream.SendNext(gameObject.transform.position);
        }
        else//is reading
        {
            //todo : sync 3 infos
            bombRigidbody.velocity = (Vector3)stream.ReceiveNext();
            gameObject.transform.position = (Vector3)stream.ReceiveNext();
        }
    }

    [PunRPC]
    public void UpdateTransform(Vector3 position, Quaternion rotation)
    {
        position.y = transform.position.y;
        bombRigidbody.MovePosition(position);
        bombRigidbody.MoveRotation(rotation);
    }

    [PunRPC]
    /// <summary>
    /// ��ɫ���ը��
    /// </summary>
    public void OnAttached(BombController bombControl)
    {
        carrier = bombControl;
    }

    [PunRPC]
    /// <summary>
    /// ��ɫ����/����ը���ȴ����¼�
    /// </summary>
    public void OnDetached()
    {
        carrier = null;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            snowPathFX.SetActive(true);
        }
    }
}
