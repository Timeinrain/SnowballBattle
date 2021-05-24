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
    public float fillDelay = 5f;           // 从炮弹踢入到大炮播放动画的时间（搬运工动画时间）
    public float fireDelay = 1.1f;         // 大炮从填充到发射炮弹的动画间隔时间
    public float fireTime = 2.15f;         // 大炮发射动画的完整时间
    public Transform bombShootPosition;    // 发射的初始位置
    public float bombShootSpeed;           // 发射的速度
    public float falseBombLifespan;        // 当假炸弹的生命周期

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
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bomb"))
        {
            if (falseBombsWatchList.Contains(other.gameObject))
                return;

            int generateNum = 3;
            for(int i = 0; i < generateNum; i++)
            {
                // 克隆原有的炸弹
                GameObject bomb = Instantiate(other.gameObject);
                bomb.GetComponent<Bomb>().SetPushable(false);
                bomb.GetComponent<Bomb>().StopExplosionCountdown();
                bomb.GetComponent<Bomb>().StopCarrierPushing();
                bomb.SetActive(false);     // 隐藏炸弹
                bombsPool.Enqueue(bomb.gameObject);
            }
            Destroy(other.gameObject);
        }
    }

    private bool isLoaded = false;  // 已经载入炮弹
    private void Update()
    {
        if (bombsPool.Count > 0 && !isLoaded)
        {
            isLoaded = true;
            StartCoroutine(ShootAllBombs());
        }

        HandleBombsLifespan();
    }

    private IEnumerator ShootAllBombs()
    {
        // 播放搬运工动画
        porterAnimator.SetTrigger("FillCannon");    
        yield return new WaitForSeconds(fillDelay);

        GameObject bomb = bombsPool.Count > 0 ? bombsPool.Dequeue() : null;
        while(bomb != null)
        {
            // 播放大炮动画
            cannonAnimator.SetTrigger("Fire");
            yield return new WaitForSeconds(fireDelay);

            // 激活炮弹并发射
            falseBombsWatchList.Add(bomb);
            bomb.SetActive(true);
            bomb.transform.position = bombShootPosition.position;
            bomb.GetComponent<Rigidbody>().velocity = new Vector3(0f, bombShootSpeed, 0f);
            yield return new WaitForSeconds(fireTime - fireDelay);

            // 获取炮弹池中下个炮弹
            bomb = bombsPool.Count > 0 ? bombsPool.Dequeue() : null;
        }

        isLoaded = false;
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
                linkedGenerator.AddBombAndGenerate(owner, 1);
            }
        }
    }

    private void OnValidate()
    {
        if(fireTime < fireDelay)
        {
            fireTime = fireDelay;
        }
    }
}
