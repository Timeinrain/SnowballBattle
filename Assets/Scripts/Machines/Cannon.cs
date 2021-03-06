using core.zqc.bombs;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public class Cannon : MonoBehaviour
{
	public BombGeneratorOnline linkedGenerator;
	public CannonAnimator cannonAnimator;
	public Animator porterAnimator;        // 炮弹搬运工
	public Team owner;

	[Header("Shoot Settings")]
	public float fillDelay = 5f;           // 从炮弹踢入到大炮播放动画的时间（搬运工动画时间）
	public Transform bombShootPosition;    // 发射的初始位置
	public float bombShootSpeed;           // 发射的速度
	public float falseBombLifespan;        // 当假炸弹的生命周期
	public GameObject bombPrefab;          // 发射的炸弹prefab

	private Queue<GameObject> bombsPool = new Queue<GameObject>();          // 等待发射的炸弹池队列
	private List<GameObject> falseBombsWatchList = new List<GameObject>();  // 监视的发射的假炸弹
	private float maxFalseBombHeight;

	private void Start()
	{
		// 计算到0速度的时间
		float upperBound = bombShootSpeed / Physics.gravity.magnitude;
		if (falseBombLifespan > upperBound)
			falseBombLifespan = upperBound;
		maxFalseBombHeight = bombShootSpeed * falseBombLifespan - 0.5f * Physics.gravity.magnitude * falseBombLifespan * falseBombLifespan;

		cannonAnimator.shootBombEvent += ShootBomb;
		cannonAnimator.animationEndEvent += HandleShootAnimationEnd;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Bomb"))
		{
			if (falseBombsWatchList.Contains(other.gameObject))
				return;

			Bomb bomb = other.gameObject.GetComponent<Bomb>();

			PlayerController player = bomb.GetLastCarrier();
			if (player != null && owner == player.GetComponent<Character>().GetTeam())
            {
				// 投入同队的大炮，得分
				player.GetComponent<Character>().ScoreFillCannon();
            }

			bomb.SetPushable(false);
			bomb.StopExplosionCountdown();
			bomb.StopCarrierPushing();
			if (bomb.snowPathFX != null)
				bomb.DetachBombPath();
			bomb.gameObject.SetActive(false);     // 隐藏炸弹
			bombsPool.Enqueue(bomb.gameObject);
		}
	}

	private bool canFire = true;
	private void Update()
	{
		if (bombsPool.Count > 0 && canFire)
		{
			canFire = false;
			StartCoroutine(ShootAllBombs());
		}

		HandleBombsLifespan();
	}

	private IEnumerator ShootAllBombs()
	{
		// 播放搬运工动画
		porterAnimator.SetTrigger("FillCannon");
		yield return new WaitForSeconds(fillDelay);

		// 播放三连射动画
		cannonAnimator.StartTripleShot();
	}

	private void ShootBomb()
	{
		GameObject bombGO = Instantiate(bombPrefab);
		Bomb bomb = bombGO.GetComponent<Bomb>();
		falseBombsWatchList.Add(bombGO);
		bomb.SetPushable(false);
		bomb.StopExplosionCountdown();
		bomb.StopCarrierPushing();
		bomb.ChangeTeam(owner);
		bombGO.transform.position = bombShootPosition.position;
		bombGO.SetActive(true);
		bombGO.GetComponent<Rigidbody>().velocity = new Vector3(0f, bombShootSpeed, 0f);
	}

	private void HandleShootAnimationEnd()
	{
		GameObject bombTemplate = bombsPool.Dequeue();
		if (PhotonNetwork.LocalPlayer.IsMasterClient)
			PhotonNetwork.Destroy(bombTemplate);
		canFire = true;
	}

	/// <summary>
	/// 处理发射的假炸弹的生命周期
	/// </summary>
	private void HandleBombsLifespan()
	{
		for (int i = falseBombsWatchList.Count - 1; i >= 0; i--)
		{
			GameObject bomb = falseBombsWatchList[i];
			if (bomb.transform.position.y > maxFalseBombHeight)
			{
				// 超过高度后销毁，并加入三个到BombGenerator
				falseBombsWatchList.RemoveAt(i);
				Destroy(bomb);
				linkedGenerator.GenerateBomb(owner, 1);
			}
		}
	}
}
