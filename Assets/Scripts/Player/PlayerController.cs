using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : UnitySingleton<PlayerController>
{
	// Start is called before the first frame update
	Rigidbody rb;
	Animator playerAnimator;
	public Camera mainCam;
	public Joystick joystick;
	[Range(1, 100)]
	public float speed;
	Vector3 movingDirRight;
	Vector3 movingDirForward;
	public Joystick viewAdjust;
	public Cinemachine.CinemachineFreeLook freeLookCam;
	void Start()
	{
		rb = GetComponent<Rigidbody>();
		playerAnimator = GetComponentInChildren<Animator>();
		movingDirForward = mainCam.transform.forward;
		movingDirRight = mainCam.transform.right;
	}

	private void FixedUpdate()
	{
		rb.velocity = (joystick.Horizontal * movingDirRight + movingDirForward * joystick.Vertical).normalized * speed + new Vector3(0, rb.velocity.y, 0);
		Vector3 dir = new Vector3(rb.velocity.x, 0, rb.velocity.z).normalized;
		if (dir.magnitude != 0)
		{
			Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);
			rb.MoveRotation(targetRotation);
		}
	}

	void AdjustDirection()
	{
		if (joystick.Horizontal == 0 && joystick.Vertical == 0)
		{
			movingDirRight = mainCam.transform.right;
			movingDirForward = new Vector3(mainCam.transform.forward.x,0,mainCam.transform.forward.z);
		}
	}

	private void Update()
	{
		playerAnimator.SetFloat("MovingSpeed", (new Vector2(rb.velocity.x, rb.velocity.z)).magnitude);
		AdjustDirection();

	}
}
