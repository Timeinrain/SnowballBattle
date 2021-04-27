using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using core.zqc.players;
/// <summary>
/// ��ҿ��ƽű����������ڲ�����
/// </summary>
public class PlayerController : MonoBehaviourPun
{
	public PlayerAnimator playerAnimator;

	private void FixedUpdate()
	{
		if (Application.platform == RuntimePlatform.WindowsEditor)
		{
			//non cameraRotation
			var xV = Input.GetAxisRaw("Horizontal");
			var yV = Input.GetAxisRaw("Vertical");
			playerAnimator.SetInput(xV, yV);
		}
	}
}
