using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pumpkinRandomJump : MonoBehaviour
{
	public float jumpInterval;
	public float jumpStrength;

	public void OnEnable()
	{
		StartCoroutine(RandomJump());
	}

	public void OnWarning()
	{
		StopAllCoroutines();
		jumpInterval /= 5;
		StartCoroutine(RandomJump());
	}

	private void Update()
	{
		//GetComponent<Rigidbody>().AddForce(Vector3.down * 50, ForceMode.Acceleration);
	}

	private void OnDisable()
	{
		StopAllCoroutines();
	}

	public IEnumerator RandomJump()
	{
		while (true)
		{
			Vector3 faceDir = new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1)).normalized;
			Vector3 jumpDir = faceDir * jumpStrength / 4 + Vector3.up * jumpStrength;
			transform.forward = faceDir;
			GetComponent<Rigidbody>().AddForce(jumpDir, ForceMode.Impulse);
			yield return new WaitForSeconds(jumpInterval + Random.Range(0, 0.5f));
		}
	}
}
