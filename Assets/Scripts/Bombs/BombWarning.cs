using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class BombWarning : MonoBehaviour
{
	public GameObject[] bombWarningFX;

	public GameObject bombRange;

	public float rotationSpeed = 3;

	public AnimationCurve bombFlash;

	public AnimationCurve ringRotationSpeed;

	public AnimationCurve ringScaling;

	public float explosionTime = 3;

	bool isOnExplosion = false;

	[Button]
	public void StartRotation()
	{
		StartCoroutine(RingRotation());
	}

	IEnumerator RingRotation()
	{
		float temp = 0;
		while (true)
		{
			temp += Time.deltaTime;
			bombRange.transform.Rotate(Vector3.forward, Time.deltaTime * rotationSpeed * ringRotationSpeed.Evaluate(temp));
			bombRange.transform.localScale = ringScaling.Evaluate(temp) * 5 * Vector3.one;
			yield return new WaitForEndOfFrame();
		}
	}

	[Button]
	public void StartExplosion()
	{
		if (!isOnExplosion)
		{
			isOnExplosion = true;
			StartCoroutine(Explosion());
			StartRotation();
		}
	}

	IEnumerator Explosion()
	{
		float temp = 0;
		while (temp < explosionTime)
		{
			temp += Time.deltaTime;
			foreach(var warningFX in bombWarningFX)
			{
				warningFX.GetComponent<MeshRenderer>().materials[1].SetFloat("_Alpha", bombFlash.Evaluate(temp));
				Color tmpColor = bombRange.GetComponent<MeshRenderer>().material.color;
				bombRange.GetComponent<MeshRenderer>().material.color = new Color(tmpColor.r, tmpColor.g, tmpColor.b, temp);
			}
			yield return new WaitForEndOfFrame();
		}

	}
}
