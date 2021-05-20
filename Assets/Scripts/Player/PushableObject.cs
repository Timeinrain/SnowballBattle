using core.zqc.bombs;
using Photon.Pun;
using UnityEngine;

public class PushableObject : MonoBehaviourPun
{
    public GameObject snowPathFX;

    bool isPushable = true;
    Rigidbody objectRigidbody;
    PushController carrier = null;

    private void Awake()
    {
        objectRigidbody = GetComponent<Rigidbody>();
    }

    private void OnDestroy()
    {
        if (carrier != null)
            carrier.DetachCurrentPushing();
    }

    [PunRPC]
    public void UpdateTransform(Vector3 position, Quaternion rotation)
    {
        position.y = transform.position.y;
        objectRigidbody.MovePosition(position);
        objectRigidbody.MoveRotation(rotation);
    }

    [PunRPC]
    /// <summary>
    /// ��ɫ���ը��
    /// </summary>
    public void OnAttached(PushController bombControl)
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
            stream.SendNext(objectRigidbody.velocity);
            stream.SendNext(gameObject.transform.position);
        }
        else//is reading
        {
            //todo : sync 3 infos
            objectRigidbody.velocity = (Vector3)stream.ReceiveNext();
            gameObject.transform.position = (Vector3)stream.ReceiveNext();
        }
    }

    //todo : sync
    /// <summary>
    /// ����/����ը��λ�ã��������Ƿ��ܷ���λ��
    /// </summary>
    /// <param name="flag"></param>
    public void SetPositionLock(bool flag)
    {
        if (flag)
        {
            objectRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }
        else
        {
            objectRigidbody.constraints = RigidbodyConstraints.None;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            snowPathFX.SetActive(true);
        }
    }

    public void SetPushable(bool flag)
    {
        isPushable = flag;
    }

    public bool CheckPushable()
    {
        return isPushable;
    }
}
