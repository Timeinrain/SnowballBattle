using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
/// <summary>
/// 玩家控制脚本，联机仍在测试中
/// </summary>
public class PlayerController : MonoBehaviourPun
{
	// Start is called before the first frame update
	Rigidbody rb;
	Animator playerAnimator;
	GameObject mainCam;
	public CameraSettings cameraSettings;
	//public Camera mainCam;
	[Range(1, 100)]
	public float speed;
	[Range(10, 1000)]
	public float rotationSpeed = 10;
	//#if UNITY_EDITOR
	//	[Header("Mobile Control Panel")]
	//	public Joystick joystick;
	//	public Joystick viewAdjust;
	//	public Cinemachine.CinemachineFreeLook freeLookCam;
	//#endif
	//Vector3 movingDirRight;
	//Vector3 movingDirForward;

	bool inAnimation = false;

	//todo: controlled by gameflow mgr
	public playMode mode;
	public enum playMode
	{
		Adventure = 0,
		PVP = 1,
	}
	void Start()
	{
		if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
		rb = GetComponent<Rigidbody>();
		playerAnimator = GetComponentInChildren<Animator>();
		//if (Application.platform == RuntimePlatform.Android)
		//{
		//	movingDirForward = mainCam.transform.forward;
		//	movingDirRight = mainCam.transform.right;
		//}
	}

	private void Awake()
	{
		if (photonView.IsMine)
		{
			mainCam = GameObject.FindGameObjectWithTag("MainCamera");
			mainCam.transform.position = cameraSettings.offset + transform.position;
			mainCam.GetComponent<CameraFollow>().lookAt = transform;
			mainCam.GetComponent<CameraFollow>().follow = transform;
			mainCam.GetComponent<CameraFollow>().offset = cameraSettings.offset;
			//mainCam.GetComponent<CameraFollow>().enabled = true;
		}
	}

	private void FixedUpdate()
	{
		//if (inAnimation) return;

		if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
		//if (Application.platform == RuntimePlatform.WindowsEditor)
		//{
		//non cameraRotation
		var xV = Input.GetAxisRaw("Horizontal");
		var yV = Input.GetAxisRaw("Vertical");
		rb.velocity = new Vector3(xV, 0, yV).normalized * speed + new Vector3(0, rb.velocity.y, 0);
		//}
		//if (Application.platform == RuntimePlatform.Android)
		//{
		//	rb.velocity = (joystick.Horizontal * movingDirRight + movingDirForward * joystick.Vertical).normalized * speed + new Vector3(0, rb.velocity.y, 0);
		//}
		Vector3 dir = new Vector3(rb.velocity.x, 0, rb.velocity.z).normalized;
		if (dir.magnitude != 0)
		{
			Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);
			Quaternion lerp = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
			rb.MoveRotation(lerp);
		}
	}

	void AdjustDirection()
	{
		//if (RuntimePlatform.Android == Application.platform)
		//	if (joystick.Horizontal == 0 && joystick.Vertical == 0)
		//	{
		//		movingDirRight = mainCam.transform.right;
		//		movingDirForward = new Vector3(mainCam.transform.forward.x, 0, mainCam.transform.forward.z);
		//	}
	}

	private void Update()
	{
		if (photonView.IsMine)
		{
			mainCam.GetComponent<CameraFollow>().UpdatePosition(transform.position);
		}
		if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
		playerAnimator.SetFloat("MovingSpeed", (new Vector2(rb.velocity.x, rb.velocity.z)).magnitude);
		//AdjustDirection();
	}

	public void SetAnimationFlag(bool flag)
	{
		inAnimation = flag;
	}
}
