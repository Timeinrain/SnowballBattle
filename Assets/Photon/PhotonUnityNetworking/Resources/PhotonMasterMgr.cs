using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// To fake a global network sync.
/// </summary>
public class PhotonMasterMgr : MonoBehaviourPun
{
	private void Awake()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			StartCoroutine(m_Update());
		}
		else
		{
			StartCoroutine(sub_Update());
		}
	}

	/// <summary>
	/// To fake master's update
	/// </summary>
	/// <returns></returns>
	private IEnumerator m_Update()
	{
		while (true)
		{
			PhotonMaterPlayerUpdate();
			yield return new WaitForEndOfFrame();
		}
	}

	/// <summary>
	/// To fake subordinates' update
	/// </summary>
	/// <returns></returns>
	private IEnumerator sub_Update()
	{
		while (true)
		{
			PhotonSubPlayerUpdate();
			yield return new WaitForEndOfFrame();
		}

	}

	/// <summary>
	/// 主机同步总控制
	/// </summary>
	private void PhotonMaterPlayerUpdate()
	{
		Debug.Log("Master Update");
	}

	/// <summary>
	/// 副机同步总控制
	/// </summary>
	private void PhotonSubPlayerUpdate()
	{
		Debug.Log("Subordinates Update");
	}

}
