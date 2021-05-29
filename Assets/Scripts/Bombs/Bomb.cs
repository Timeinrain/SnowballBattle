using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Sirenix.OdinInspector;
using UnityEngine.UI;

namespace core.zqc.bombs
{
	[RequireComponent(typeof(Rigidbody))]
	public class Bomb : PushableObject, IPunInstantiateMagicCallback
	{
		public float explosionTime;
		public float explosionRange;
		public float explosionTTL = 0.5f;
		bool freezeCountdown = true;
		float countdownTimer = 0f;
		bool hasExploded = false;

		public GameObject traceFX;

		public GameObject bombWarningFX;

		public GameObject ExplosionFx;

		public MeshRenderer meshRenderer;
		public Material redTeamMaterial;
		public Material blueTeamMaterial;

		public Text textCountdown;

		List<Team> friendlyList = new List<Team>();

		private void OnDrawGizmos()
		{
			Gizmos.DrawWireSphere(transform.position, explosionRange);
		}

		protected override void Awake()
		{
			base.Awake();
			type = CarryType.Bomb;
		}

		[Button]
		public void StartCountDown()
		{
			freezeCountdown = false;
			countdownTimer = 5;
		}

		private void Update()
		{
			if (freezeCountdown)
			{
				textCountdown.text = "";
			}
			else
			{
				countdownTimer -= Time.deltaTime;
				//fresnel 2-0  speed 0.5 - 3
				if (countdownTimer <= 3)
				{
					GetComponent<BombWarning>().StartWarning();
				}

				// 显示倒计时
				int displayNum = (int)(countdownTimer + 0.99f);
				textCountdown.text = (displayNum.ToString());

				if (countdownTimer <= 0f && !hasExploded)
				{
					hasExploded = true;
					Explode();
				}
			}
		}

		public void OnPhotonInstantiate(PhotonMessageInfo info)
		{
			object[] data = info.photonView.InstantiationData;
			Vector3 initialVelocity = (Vector3)data[0];
			Team owner = (Team)data[1];
			GetComponent<Rigidbody>().velocity = initialVelocity;
			ChangeTeam(owner);
			countdownTimer = explosionTime;
			freezeCountdown = false; // 开始计时
		}

		private void Explode()
		{
			if (PhotonNetwork.LocalPlayer.IsMasterClient)
			{
				// 非主机不处理角色游戏逻辑，只接受主机广播
				Collider[] cols = Physics.OverlapSphere(transform.position, explosionRange, LayerMask.GetMask("Player"));
				foreach (var col in cols)
				{
					Character character = col.GetComponent<Character>();
					if (character != null)
					{
						if (friendlyList.Contains(character.GetTeam())) continue;
						character.photonView.RPC("TakeDamage", RpcTarget.All, 1);
					}
				}
			}
			if (traceFX != null)
				traceFX.GetComponent<BombPathController>().Detach();
			StartCoroutine(ExplosionFxPlay());
		}

		/// <summary>
		/// Explosion FX
		/// </summary>
		IEnumerator ExplosionFxPlay()
		{
			ExplosionFx.SetActive(true);
			ExplosionFx.transform.SetParent(null);
			yield return null;
			Destroy(gameObject);
		}

		[PunRPC]
		public void DelayShoot(Vector3 speed, float delay)
		{
			StartCoroutine(Shoot(speed, delay));
		}

		/// <summary>
		/// Set Lock Sync For online 
		/// </summary>
		/// <param name="speed"></param>
		/// <param name="delay"></param>
		/// <returns></returns>
		IEnumerator Shoot(Vector3 speed, float delay)
		{
			SetPositionLock(true);             // 防止动画期间炸弹发生位移
			yield return new WaitForSeconds(delay);
			SetPositionLock(false);
			GetComponent<Rigidbody>().AddForce(speed, ForceMode.VelocityChange);
		}

		[PunRPC]
		public void StopExplosionCountdown()
		{
			freezeCountdown = true;
		}

		public void DetachBombPath()
		{
			GetComponentInChildren<BombPathController>().Detach();
		}

		/// <summary>
		/// 改变队伍属性并更换贴图
		/// </summary>
		/// <param name="team"></param>
		public void ChangeTeam(Team team)
		{
			if (!friendlyList.Contains(team))
				friendlyList.Add(team);

			switch (team)
			{
				case Team.Blue:
					meshRenderer.material = blueTeamMaterial;
					break;
				case Team.Red:
					meshRenderer.material = redTeamMaterial;
					break;
				case Team.Yellow:
					break;
				case Team.Green:
					break;
				case Team.Null:
					break;
			}
		}

		/// <summary>
		/// 在加入photon view 时需要发送的message
		/// 将所有要同步的信息在期间写好
		/// 包括：
		/// 1.炸弹的位置信息
		/// 2.炸弹的归属信息
		/// 3.炸弹的爆炸信息
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="info"></param>
		public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
		{
			if (stream.IsWriting)
			{
				//todo : 3 infos
				stream.SendNext(GetComponent<Rigidbody>().velocity);
				stream.SendNext(gameObject.transform.position);
			}
			else//is reading
			{
				//todo : sync 3 infos
				GetComponent<Rigidbody>().velocity = (Vector3)stream.ReceiveNext();
				gameObject.transform.position = (Vector3)stream.ReceiveNext();
			}
		}


		private void OnCollisionEnter(Collision collision)
		{
			if (collision.gameObject.CompareTag("Ground"))
			{
				snowPathFX.SetActive(true);
			}
		}
	}
}