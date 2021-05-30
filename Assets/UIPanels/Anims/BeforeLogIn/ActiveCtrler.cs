using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveCtrler : MonoBehaviour
{
    public void SetActiveFalse()
	{
		StartCoroutine(InActive());
	}

	IEnumerator InActive()
	{
		yield return new WaitForSeconds(0.5f);
		gameObject.SetActive(false);
	}
}
