using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class InputHandler : MonoBehaviourPun
{

	PlayerController playerController;

	[Header("Keycodes for Actions")]
	[SerializeField] public KeyCode startPush;
	[SerializeField] public KeyCode kick;
	[SerializeField] public KeyCode fire;

	private void Awake()
	{
		if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
		playerController = gameObject.GetComponent<PlayerController>();
	}

	void Update()
    {
		if (PhotonNetwork.IsConnected && !photonView.IsMine) return;

		if (Input.GetKeyDown(kick))
		{
			playerController.Kick();
		}
		if (Input.GetKeyDown(startPush))
		{
			playerController.StartPush();
		}
	}

	private void FixedUpdate()
	{
		if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
		var xV = Input.GetAxisRaw("Horizontal");
		var yV = Input.GetAxisRaw("Vertical");
		playerController.Move(xV, yV);
	}
}
