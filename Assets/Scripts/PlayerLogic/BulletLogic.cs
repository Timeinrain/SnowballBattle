using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace networkTest
{
	public class BulletLogic : MonoBehaviour
	{
		// Start is called before the first frame update
		float destroyTime = 3f;
		void Start()
		{
			StartCoroutine(DestroySelf());
		}

		// Update is called once per frame
		void Update()
		{

		}

		IEnumerator DestroySelf()
		{
			float curr = 0;
			while (curr < destroyTime)
			{
				curr += Time.deltaTime;
				yield return new WaitForEndOfFrame();
			}
			Destroy(gameObject);
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (collision.gameObject.CompareTag("Player")) return;
			Destroy(gameObject);
		}
	}
}