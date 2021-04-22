using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerControllerNetworkTest : MonoBehaviourPun
{
	Rigidbody rb;
	float xSpeed, ySpeed;
	[SerializeField] bool isJump;
	[SerializeField] bool isOnGround;
	// Start is called before the first frame update
	void Start()
	{
		rb = GetComponent<Rigidbody>();
		isJump = false;
		isOnGround = false;
	}

	// Update is called once per frame
	void Update()
	{
		if (!photonView.IsMine && PhotonNetwork.IsConnected)
		{
			return;
		}
		xSpeed = Input.GetAxisRaw("Horizontal");
		ySpeed = Input.GetAxisRaw("Vertical");
		RaycastHit raycast;
		if (Physics.Raycast(gameObject.transform.position, Vector3.down, out raycast, transform.localScale.y * 0.7f, LayerMask.GetMask("Ground")))
		{
			if (raycast.collider.gameObject)
			{
				isOnGround = true;
			}
		}
		else
		{
			isOnGround = false;
		}
		if (isOnGround)
		{
			isJump = Input.GetKeyDown(KeyCode.Space);
		}
		if (isJump)
		{
			rb.AddForce(Vector3.up * 10f, ForceMode.Impulse);
			isJump = false;
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawLine(transform.position, transform.position + Vector3.down * transform.localScale.y * 0.7f);
	}

	private void FixedUpdate()
	{
		if (PhotonNetwork.IsConnected && !photonView.IsMine)
		{
			return;
		}
		rb.velocity = new Vector3(xSpeed, 0, ySpeed).normalized * 2 + new Vector3(0, rb.velocity.y, 0);
	}
}
