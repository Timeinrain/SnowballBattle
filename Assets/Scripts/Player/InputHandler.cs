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

	private bool isMouseDown = false;
	private float chargeTimer = 0f;     // ��¼����ʱ��

	private void Awake()
	{
		if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
		playerController = gameObject.GetComponent<PlayerController>();
	}

	void Update()
    {
		if (PhotonNetwork.IsConnected && !photonView.IsMine) return;

		if (Input.GetMouseButtonDown(0))
		{
			// ������
			isMouseDown = true;
			chargeTimer = 0f;
			playerController.Kick();
		}
        if (Input.GetMouseButtonUp(0) || chargeTimer >= playerController.maxChargeTime)
        {
			// �ɿ�����򵽴�����ʱ������ǿ���ɿ�
			isMouseDown = false;
			playerController.Throw(chargeTimer);
			chargeTimer = 0f;
		}
		if (Input.GetKeyDown(startPush))
		{
			playerController.ChangePushState();
		}
        if (Input.GetKeyDown(fire))
        {
			// ���������ʱ����
        }
	}

	private void FixedUpdate()
	{
		if (PhotonNetwork.IsConnected && !photonView.IsMine) return;

        if (isMouseDown)
        {
			chargeTimer += Time.fixedDeltaTime;
        }
		var xV = Input.GetAxisRaw("Horizontal");
		var yV = Input.GetAxisRaw("Vertical");
		playerController.Move(xV, yV);
	}
}
