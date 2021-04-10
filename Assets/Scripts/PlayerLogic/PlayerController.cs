using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace networkTest
{
	public class PlayerController : MonoBehaviour
	{
		Vector3 velocity;
		public GameObject attack;
		public float rotationSpeed = 10;
		[Range(10, 100)]
		public float attackSpeed = 20;
		public float bulletOffset = 0.5f;
		public float attackRate = 0.2f;
		Rigidbody rb;
		[Range(0, 100)]
		public float speed = 10;
		bool isAttackable = true;
		void Start()
		{
			rb = GetComponent<Rigidbody>();
		}

		// Update is called once per frame
		void Update()
		{
			float vx, vy;
			vx = Input.GetAxisRaw("Horizontal");
			vy = Input.GetAxisRaw("Vertical");
			Vector3 dir = new Vector3(vx, transform.forward.y, vy);
			if (dir.magnitude >= 0.1)
				transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotationSpeed);
			velocity = new Vector3(0, rb.velocity.y, 0) + new Vector3(vx, 0, vy).normalized * speed;
			if (Input.GetKey(KeyCode.J) && isAttackable)
			{
				StartCoroutine(bulletTimer());
				isAttackable = false;
				Attack();
			}
		}
		IEnumerator bulletTimer()
		{
			var time = 0f;
			while (time + Time.deltaTime <= attackRate)
			{
				time += Time.deltaTime;
				yield return new WaitForEndOfFrame();
			}
			isAttackable = true;
		}
		void Attack()
		{
			var bulletTrans = transform.position + transform.forward * bulletOffset;
			GameObject bullet = Instantiate(attack, bulletTrans, Quaternion.identity);
			bullet.transform.SetParent(null);
			bullet.GetComponent<Rigidbody>().AddForce(transform.forward * attackSpeed, ForceMode.Impulse);
		}

		private void FixedUpdate()
		{
			rb.velocity = velocity;
		}

	}
}