using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 相机跟随脚本，联机仍在测试中
/// </summary>
public class CameraFollow : MonoBehaviour
{
	// Start is called before the first frame update
	Camera cam;
	public Transform lookAt = null;
	public Transform follow = null;
	public Vector3 offset;
	[Range(0.5f, 5)]
	public float followSpeed = 2;
	Vector3 initDir;
	float dirMod;
	private void Awake()
	{

		cam = GetComponent<Camera>();
		if (follow)
		{
			initDir = -follow.position + cam.transform.position;
			dirMod = initDir.magnitude;
		}
	}
	public void UpdatePosition(Vector3 position)
	{
		if (initDir.magnitude != 0)
		{
			cam.transform.position = Vector3.Slerp(cam.transform.position, position + initDir, followSpeed * Time.deltaTime);
		}
		else
		{
			cam.transform.position = Vector3.Slerp(cam.transform.position, position + offset, followSpeed * Time.deltaTime);
		}
	}
}
