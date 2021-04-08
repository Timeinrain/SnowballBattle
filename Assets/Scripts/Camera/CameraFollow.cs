using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	// Start is called before the first frame update
	Camera cam;
	public Transform lookAt;
	public Transform follow;
	[Range(0.5f, 5)]
	public float followSpeed = 2;
	Vector3 initDir;
	float dirMod;
	private void Awake()
	{
		cam = GetComponent<Camera>();
		initDir = -follow.position + cam.transform.position;
		dirMod = initDir.magnitude;
	}
	private void FixedUpdate()
	{
		cam.transform.position = Vector3.Slerp(cam.transform.position, follow.position + initDir / initDir.magnitude * dirMod, followSpeed * Time.deltaTime);
	}

	// Update is called once per frame
	void Update()
	{

	}
}
