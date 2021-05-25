using core.zqc.bombs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public class Cannon : MonoBehaviour
{
    public BombGeneratorOnline linkedGenerator;
    public CannonAnimator cannonAnimator;
    public Animator porterAnimator;        // �ڵ����˹�
    public Team owner;

    [Header("Shoot Settings")]
    public float fillDelay = 5f;           // ���ڵ����뵽���ڲ��Ŷ�����ʱ�䣨���˹�����ʱ�䣩
    public Transform bombShootPosition;    // ����ĳ�ʼλ��
    public float bombShootSpeed;           // ������ٶ�
    public float falseBombLifespan;        // ����ը������������
    public GameObject bombPrefab;          // �����ը��prefab

    private Queue<GameObject> bombsPool = new Queue<GameObject>();          // �ȴ������ը���ض���
    private List<GameObject> falseBombsWatchList = new List<GameObject>();  // ���ӵķ���ļ�ը��
    private float maxFalseBombHeight;

    private void Start()
    {
        // ���㵽0�ٶȵ�ʱ��
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

            other.gameObject.GetComponent<Bomb>().SetPushable(false);
            other.gameObject.GetComponent<Bomb>().StopExplosionCountdown();
            other.gameObject.GetComponent<Bomb>().StopCarrierPushing();
            other.gameObject.SetActive(false);     // ����ը��
            bombsPool.Enqueue(other.gameObject);
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
        // ���Ű��˹�����
        porterAnimator.SetTrigger("FillCannon");    
        yield return new WaitForSeconds(fillDelay);

        // ���������䶯��
        cannonAnimator.StartTripleShot();
    }

    private void ShootBomb()
    {
        GameObject bomb = Instantiate(bombPrefab);
        falseBombsWatchList.Add(bomb); 
        bomb.GetComponent<Bomb>().SetPushable(false);
        bomb.GetComponent<Bomb>().StopExplosionCountdown();
        bomb.GetComponent<Bomb>().StopCarrierPushing();
        bomb.transform.position = bombShootPosition.position;
        bomb.SetActive(true);
        bomb.GetComponent<Rigidbody>().velocity = new Vector3(0f, bombShootSpeed, 0f);
    }

    private void HandleShootAnimationEnd()
    {
        GameObject bombTemplate = bombsPool.Dequeue();
        Destroy(bombTemplate);
        canFire = true;
    }

    /// <summary>
    /// ������ļ�ը������������
    /// </summary>
    private void HandleBombsLifespan()
    {
        for(int i = falseBombsWatchList.Count - 1; i >= 0; i--)
        {
            GameObject bomb = falseBombsWatchList[i];
            if (bomb.transform.position.y > maxFalseBombHeight)
            {
                // �����߶Ⱥ����٣�������������BombGenerator
                falseBombsWatchList.RemoveAt(i);
                Destroy(bomb);
                linkedGenerator.AddBombAndGenerate(owner, 1);
            }
        }
    }
}
