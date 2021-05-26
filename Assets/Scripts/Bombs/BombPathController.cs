using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombPathController : MonoBehaviour
{
	GameObject target;
	Vector3 offset;
	void Start()
	{
		target = transform.parent.gameObject;
		offset = target.transform.position - transform.position;
	}

	public void Detach()
	{
		transform.SetParent(null);
		StartCoroutine(DestroySelf());
	}

	IEnumerator DestroySelf()
	{
		yield return new WaitForSeconds(5f);
		Destroy(gameObject);
	}
}
