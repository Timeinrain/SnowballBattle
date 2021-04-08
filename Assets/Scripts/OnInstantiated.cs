using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class OnInstantiated : MonoBehaviourPun
{
	// Start is called before the first frame update
	public Camera mainCam;
	void Start()
	{
		if (photonView.IsMine && PhotonNetwork.IsConnected)
		{
			if (GameObject.FindGameObjectWithTag("MainCamera"))
				GameObject.FindWithTag("MainCamera").GetComponent<Camera>().targetDisplay = 2;
			GameObject camGO =
			PhotonNetwork.Instantiate("Camera", mainCam.transform.position, mainCam.transform.rotation, 0);
			camGO.GetComponent<CameraFollow>().follow = gameObject.transform;
			camGO.GetComponent<CameraFollow>().lookAt = gameObject.transform;
		}
	}

	// Update is called once per frame
	void Update()
	{

	}
}
