using core.zqc.bombs;
using Photon.Pun;
using UnityEngine;

public class PushableObject : MonoBehaviourPun
{
	public enum CarryType
	{
		Null,
		Bomb,
		Player,
	}

	public GameObject snowPathFX;

	bool isPushable = true;
	Rigidbody objectRigidbody;
	PlayerController carrier = null;
	PlayerController lastCarrier = null;
	public CarryType type { get; set; }

	protected virtual void Awake()
	{
		objectRigidbody = GetComponent<Rigidbody>();
		type = CarryType.Null;
	}

	private void OnDestroy()
	{
		if (carrier != null)
			carrier.StopPushing();
	}


	/// <summary>
	/// 更改这个函数==============================================
	/// </summary>
	/// <param name="position"></param>
	/// <param name="rotation"></param>
	[PunRPC]
	public void UpdateTransform(Vector3 position, Quaternion rotation)
	{
		position.y = transform.position.y;
		objectRigidbody.MovePosition(position);
		objectRigidbody.MoveRotation(rotation);

		if (type == CarryType.Bomb)
			objectRigidbody.transform.up = Vector3.up;
	}

	[PunRPC]
	/// <summary>
	/// 附加到角色
	/// </summary>
	public void Attach(PlayerController owner)
	{
		if (type == CarryType.Bomb)
        {
			owner.GetComponent<Character>().ScoreGetBomb();
        }
		if (carrier != null)
			carrier.StopPushing();
		carrier = owner;
		lastCarrier = owner;
		objectRigidbody.gameObject.GetPhotonView().RequestOwnership();
	}

	[PunRPC]
	/// <summary>
	/// 角色发射/丢弃炸弹等触发事件
	/// </summary>
	public void Detach()
	{
		if (carrier != null)
		{
			carrier.pushController.DetachCurrentPushing();
			carrier = null;
		}
		objectRigidbody.gameObject.GetPhotonView().TransferOwnership(PhotonNetwork.MasterClient);
	}

	public void StopCarrierPushing()
	{
		if (carrier != null)
		{
			carrier.StopPushing();
			carrier = null;
		}
	}

	//todo : sync
	/// <summary>
	/// 锁定/解锁炸弹位置，控制其是否能发生位移
	/// </summary>
	/// <param name="flag"></param>
	public void SetPositionLock(bool flag)
	{
		return; // TODO: 是否需要锁定
		if (flag)
		{
			objectRigidbody.constraints = RigidbodyConstraints.FreezeAll;
		}
		else
		{
			objectRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag("Ground"))
		{
			if(snowPathFX!=null)
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

	public PlayerController GetLastCarrier()
    {
		return lastCarrier;
    }

	/// <summary>
	/// 检查是否正在被推动
	/// </summary>
	/// <returns></returns>
	public bool CheckBeingPushed()
    {
		return carrier != null;
    }
}
