using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Set 2 Dynamic Animation of the model.
/// </summary>
public class MiniModelFloating : MonoBehaviour
{
	public float rotationSpeed = 30;
	public float floatingSpeed = 0.5f;
	public Transform rotateAround;
	Vector3 initPos;
	bool flag = false;
	public float floatingStrength = 0.5f;
	void Start()
	{
		initPos = transform.position;
		StartCoroutine(Rotation());
		StartCoroutine(Floating());
	}

	IEnumerator Rotation()
	{
		while (true)
		{
			transform.RotateAround(rotateAround.transform.position, Vector3.up, Time.deltaTime * rotationSpeed);
			yield return new WaitForEndOfFrame();
		}
	}

	IEnumerator Floating()
	{
		Vector3 target = initPos + Vector3.up * floatingStrength;
		while (true)
		{
			transform.position += flag ? Vector3.up * floatingSpeed * Time.deltaTime : -1 * Vector3.up * floatingSpeed * Time.deltaTime;
			if (Mathf.Abs(transform.position.y - initPos.y) > floatingStrength)
			{
				flag = !flag;
			}
			yield return new WaitForEndOfFrame();
		}
	}
}
