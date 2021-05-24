using core.zqc.bombs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public class Cannon : MonoBehaviour
{
    public BombGeneratorOnline linkedGenerator;
    public Animator cannonAnimator;
    public Animator porterAnimator;        // 炮弹搬运工
    public Team owner;

    [Header("Shoot Settings")]
    public float shootCycleTime;           // 炮弹发射处理流程最短间隔时间，必须大于以下两项之和
    public float fillDelay = 5f;           // 从炮弹踢入到大炮播放动画的时间（搬运工动画时间）
    public float fireDelay = 1.1f;         // 大炮从填充到发射炮弹的动画间隔时间
    public Transform bombShootPosition;    // 发射的初始位置
    public float bombShootSpeed;           // 发射的速度
    public float maxFalseBombHeight;       // 当假炸弹超过此高度时就销毁

    private Queue<GameObject> bombsPool = new Queue<GameObject>();          // 等待发射的炸弹池队列
    private List<GameObject> falseBombsWatchList = new List<GameObject>();  // 监视的发射的假炸弹

    private void Start()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bomb"))
        {
            if (falseBombsWatchList.Contains(other.gameObject))
                return;
            bombsPool.Enqueue(other.gameObject);
            other.GetComponent<Bomb>().StopCarrierPushing();
            other.gameObject.gameObject.SetActive(false);  // 隐藏炸弹
        }
    }

    private void Fire()
    {
        Debug.Log("fire!!!!!!");
    }

    private float shootTimer = 0f;
    private bool canFire = true;
    private void Update()
    {
        // 计时发射冷却
        if (!canFire)
        {
            if (shootTimer >= shootCycleTime)
            {
                shootTimer = 0f;
                canFire = true;
            }
            else
            {
                shootTimer += Time.deltaTime;
            }            
        }

        if (canFire && bombsPool.Count > 0)
        {
            // 对新的炮弹执行一系列动画
            GameObject bomb = bombsPool.Dequeue();
            StartCoroutine(ShootFalseBomb(bomb));
            canFire = false;
        }

        HandleBombsLifespan();
    }

    private IEnumerator ShootFalseBomb(GameObject bomb)
    {
        // 播放搬运工动画
        porterAnimator.SetTrigger("FillCannon");    
        yield return new WaitForSeconds(fillDelay);

        // 播放大炮动画
        cannonAnimator.SetTrigger("Fire");          
        yield return new WaitForSeconds(fireDelay);

        // 激活炮弹并发射
        falseBombsWatchList.Add(bomb);
        bomb.SetActive(true);
        bomb.transform.position = bombShootPosition.position;
        bomb.GetComponent<Rigidbody>().velocity = new Vector3(0f, bombShootSpeed, 0f);
    }

    /// <summary>
    /// 处理发射的假炸弹的生命周期
    /// </summary>
    private void HandleBombsLifespan()
    {
        for(int i = falseBombsWatchList.Count - 1; i >= 0; i--)
        {
            GameObject bomb = falseBombsWatchList[i];
            if (bomb.transform.position.y > maxFalseBombHeight)
            {
                // 超过高度后销毁，并加入三个到BombGenerator
                falseBombsWatchList.RemoveAt(i);
                Destroy(bomb);
                linkedGenerator.AddBombAndGenerate(owner, 3);
            }
        }
    }

    private void OnValidate()
    {
        if (shootCycleTime < fillDelay + fireDelay)
        {
            shootCycleTime = fillDelay + fireDelay;
        }
    }
}
