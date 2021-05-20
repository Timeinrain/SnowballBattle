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
    /// 角色获得炸弹
    /// </summary>
    public void OnAttached(PushController bombControl)
    {

        carrier = bombControl;
    }
    [PunRPC]
    /// <summary>
    /// 角色发射/丢弃炸弹等触发事件
    /// </summary>
    public void OnDetached()
    {
        carrier = null;
    }

    /// <summary>
    /// 在加入photon view 时需要发送的message
    /// 将所有要同步的信息在期间写好
    /// 包括：
    /// 1.炸弹的位置信息
    /// 2.炸弹的归属信息
    /// 3.炸弹的爆炸信息
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
    /// 锁定/解锁炸弹位置，控制其是否能发生位移
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
