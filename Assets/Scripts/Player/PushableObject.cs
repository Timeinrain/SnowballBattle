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
    /// 角色获得炸弹
    /// </summary>
    public void OnAttached(BombController bombControl)
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            snowPathFX.SetActive(true);
        }
    }
}
