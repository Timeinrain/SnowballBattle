using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombFXDestroy : MonoBehaviour
{
	private void OnEnable()
	{
		StartCoroutine(DestroySelf());
	}

	IEnumerator DestroySelf()
	{
		yield return new WaitForSeconds(0.95f);
		Destroy(gameObject);
	}
}
