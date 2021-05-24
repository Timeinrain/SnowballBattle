using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerCamera : MonoBehaviourPun
{
	GameObject mainCam;
	public Cinemachine.CinemachineVirtualCamera virtualCam;

	private void Awake()
	{
		if (photonView.IsMine || !PhotonNetwork.IsConnected)
		{
			virtualCam = FindObjectOfType<Cinemachine.CinemachineVirtualCamera>();
			mainCam = GameObject.FindGameObjectWithTag("MainCamera");
			mainCam.GetComponent<CameraFollow>().SetTransform(transform.position);
			mainCam.GetComponent<CameraFollow>().lookAt = transform;
			mainCam.GetComponent<CameraFollow>().follow = transform;
			mainCam.GetComponent<CameraFollow>().player = gameObject;
		}
	}

	void Update()
	{
		if (photonView.IsMine || !PhotonNetwork.IsConnected)
		{
			mainCam.GetComponent<CameraFollow>().UpdatePosition(transform.position);
		}
	}
}
