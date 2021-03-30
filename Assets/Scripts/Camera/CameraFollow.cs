using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	// Start is called before the first frame update
	Camera cam;
	public Transform lookAt;
	public Transform follow;
	public Joystick adjustView;
	public float viewAdjustCoef;
	Vector3 initDir;
	float dirMod;
	private void Awake()
	{
		cam = GetComponent<Camera>();
		initDir = -follow.position + cam.transform.position;
		dirMod = initDir.magnitude;
	}
	private void OnEnable()
	{

	}
	void Start()
	{

	}
	private void FixedUpdate()
	{
		cam.transform.position = Vector3.Slerp(cam.transform.position, follow.position + initDir / initDir.magnitude * dirMod, Time.deltaTime);
	}

	// Update is called once per frame
	void Update()
	{
		cam.transform.RotateAround(follow.position, Vector3.up, adjustView.Horizontal * viewAdjustCoef);
		cam.transform.RotateAround(follow.position, Vector3.right, adjustView.Vertical * viewAdjustCoef);
		cam.transform.LookAt(lookAt);
		initDir = -follow.position + cam.transform.position;

	}
}
